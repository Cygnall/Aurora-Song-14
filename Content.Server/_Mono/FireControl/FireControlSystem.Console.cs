// SPDX-FileCopyrightText: 2025 Ark
// SPDX-FileCopyrightText: 2025 ark1368
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission

using Content.Server.Administration.Logs;
using Content.Server.Shuttles.Systems;
using Content.Shared._Mono.FireControl;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;

namespace Content.Server._Mono.FireControl;

public sealed partial class FireControlSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConsoleSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private bool _completedCheck = false;
    private void InitializeConsole()
    {
        SubscribeLocalEvent<FireControlConsoleComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<FireControlConsoleComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<FireControlConsoleComponent, FireControlConsoleRefreshServerMessage>(OnRefreshServer);
        SubscribeLocalEvent<FireControlConsoleComponent, FireControlConsoleFireMessage>(OnFire);
        SubscribeLocalEvent<FireControlConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnPowerChanged(EntityUid uid, FireControlConsoleComponent component, PowerChangedEvent args)
    {
        if (args.Powered)
            TryRegisterConsole(uid, component);
        else
            UnregisterConsole(uid, component);
    }

    private void OnComponentShutdown(EntityUid uid, FireControlConsoleComponent component, ComponentShutdown args)
    {
        UnregisterConsole(uid, component);
    }

    private void OnRefreshServer(EntityUid uid, FireControlConsoleComponent component, FireControlConsoleRefreshServerMessage args)
    {
        if (component.ConnectedServer == null)
        {
            TryRegisterConsole(uid, component);
        }

        if (component.ConnectedServer != null &&
            TryComp<FireControlServerComponent>(component.ConnectedServer, out var server) &&
            server.ConnectedGrid != null)
        {
            RefreshControllables((EntityUid)server.ConnectedGrid);
        }
    }

    private void OnFire(EntityUid uid, FireControlConsoleComponent component, FireControlConsoleFireMessage args)
    {
        if (component.ConnectedServer == null || !TryComp<FireControlServerComponent>(component.ConnectedServer, out var server))
            return;

        var xform = Transform(uid);
        var grid = xform.GridUid;
        if (grid == null)
            return;

        // Fire the actual weapons
        FireWeapons((EntityUid)component.ConnectedServer, args.Selected, args.Coordinates, server);

        if ((component.NextLog == null || component.NextLog < _timing.CurTime) && args.Selected.Any())
        {
            var firePos = _transform.ToMapCoordinates(GetCoordinates(args.Coordinates)).Position;
            var ourPos = _transform.GetWorldPosition(grid.Value);
            var grids = new List<Entity<MapGridComponent>>();
            var adjust = new Vector2(component.LogGridLookupRange, component.LogGridLookupRange);
            _mapMan.FindGridsIntersecting(xform.MapID, new Box2(firePos - adjust, firePos + adjust), ref grids, approx: true, includeMap: false);
            grids.RemoveAll(g => g == grid);
            EntityUid? closest = null;
            foreach (var gridUid in grids)
            {
                var newPos = _transform.GetWorldPosition(gridUid);
                if (closest == null || (newPos - firePos).LengthSquared() < (_transform.GetWorldPosition(closest.Value) - firePos).LengthSquared())
                    closest = gridUid;
            }

            _adminLogger.Add(LogType.ShipgunFired, LogImpact.High,
                    $"{ToPrettyString(args.Actor):user} fired weaponry of ship {ToPrettyString(grid):entity} from ({ourPos}) to ({firePos}), closest grid: {ToPrettyString(closest)}");

            component.NextLog = _timing.CurTime + component.LogSpacing;
        }

        UpdateUi(uid, component);
    }

    public void OnUIOpened(EntityUid uid, FireControlConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateUi(uid, component);
    }

    private void UnregisterConsole(EntityUid console, FireControlConsoleComponent? component = null)
    {
        if (!Resolve(console, ref component))
            return;

        if (component.ConnectedServer == null || !TryComp<FireControlServerComponent>(component.ConnectedServer, out var server))
            return;

        server.Consoles.Remove(console);
        component.ConnectedServer = null;
        UpdateUi(console, component);
    }
    private bool TryRegisterConsole(EntityUid console, FireControlConsoleComponent? consoleComponent = null)
    {
        if (!Resolve(console, ref consoleComponent))
            return false;

        var gridServer = TryGetGridServer(console);

        if (gridServer.ServerComponent == null)
            return false;

        if (gridServer.ServerComponent.Consoles.Add(console))
        {
            consoleComponent.ConnectedServer = gridServer.ServerUid;
            UpdateUi(console, consoleComponent);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateUi(EntityUid uid, FireControlConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        NavInterfaceState navState = _shuttleConsoleSystem.GetNavState(uid, _shuttleConsoleSystem.GetAllDocks());

        List<FireControllableEntry> controllables = new();
        if (component.ConnectedServer != null && TryComp<FireControlServerComponent>(component.ConnectedServer, out var server))
        {
            foreach (var controllable in server.Controlled)
            {
                var controlled = new FireControllableEntry();
                controlled.NetEntity = EntityManager.GetNetEntity(controllable);
                controlled.Coordinates = GetNetCoordinates(Transform(controllable).Coordinates);
                controlled.Name = MetaData(controllable).EntityName;

                var (ammoCount, hasManualReload) = GetWeaponAmmunitionInfo(controllable);
                controlled.AmmoCount = ammoCount;
                controlled.HasManualReload = hasManualReload;

                controllables.Add(controlled);
            }
        }

        var array = controllables.ToArray();

        var state = new FireControlConsoleBoundInterfaceState(component.ConnectedServer != null, array, navState);
        _ui.SetUiState(uid, FireControlConsoleUiKey.Key, state);
    }

    /// <summary>
    /// Gets ammo information for a weapon to determine if it has manual reload.
    /// </summary>
    private (int? ammoCount, bool hasManualReload) GetWeaponAmmunitionInfo(EntityUid weaponEntity)
    {
        if (TryComp<BasicEntityAmmoProviderComponent>(weaponEntity, out var basicAmmo))
        {
            var hasRecharge = HasComp<RechargeBasicEntityAmmoComponent>(weaponEntity);

            return (basicAmmo.Count, !hasRecharge);
        }

        if (TryComp<BallisticAmmoProviderComponent>(weaponEntity, out var ballisticAmmo))
        {
            return (ballisticAmmo.Count, ballisticAmmo.Cycleable);
        }

        return (null, false);
    }
}
