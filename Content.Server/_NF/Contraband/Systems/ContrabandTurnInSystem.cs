using System.Linq;
using Content.Server._NF.Cargo.Systems;
using Content.Server._NF.Contraband.Components;
using Content.Server.Cargo.Components;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Server.Station.Systems;
using Content.Shared._AS.Contraband.Events; // Aurora
using Content.Shared._AS.Contraband.ScuOutput; // Aurora
using Content.Shared._AS.License; // Aurora
using Content.Shared._AS.Weapons.Ranged.Systems; // Aurora Song
using Content.Shared._NF.Contraband.BUI;
using Content.Shared._NF.Contraband.Components;
using Content.Shared._NF.Contraband.Events;
using Content.Shared._NF.Contraband;
using Content.Shared.Access.Systems;
using Content.Shared.Contraband;
using Content.Shared.Coordinates;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Stacks;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Map; // Aurora
using Robust.Shared.Prototypes;

namespace Content.Server._NF.Contraband.Systems;

/// <summary>
/// Contraband system. Contraband Pallet UI Console is mostly a copy of the system in cargo. Checkraze Note: copy of my code from cargosystems.shuttles.cs
/// </summary>
public sealed partial class ContrabandTurnInSystem : SharedContrabandTurnInSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly AudioSystem _audio = default!; // Aurora
    [Dependency] private readonly AccessReaderSystem _reader = default!; // Aurora
    [Dependency] private readonly PopupSystem _popup = default!; // Aurora
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!; // Aurora
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!; // Aurora
    [Dependency] private readonly FirearmSerialNumberSystem _serialSystem = default!; // Aurora Song

    private EntityQuery<MobStateComponent> _mobQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<CargoSellBlacklistComponent> _blacklistQuery;

    private EntityUid _scuOutput;

    public override void Initialize()
    {
        base.Initialize();

        _xformQuery = GetEntityQuery<TransformComponent>();
        _blacklistQuery = GetEntityQuery<CargoSellBlacklistComponent>();
        _mobQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<ContrabandPalletConsoleComponent, ContrabandPalletSellMessage>(OnPalletSale);
        SubscribeLocalEvent<ContrabandPalletConsoleComponent, ContrabandPalletAppraiseMessage>(OnPalletAppraise);
        SubscribeLocalEvent<ContrabandPalletConsoleComponent, BoundUIOpenedEvent>(OnPalletUIOpen);
        SubscribeLocalEvent<ContrabandPalletConsoleComponent, ContrabandPalletRegisterMessage>(OnPalletRegister);
        SubscribeLocalEvent<ScuOutputComponent, ComponentInit>(OnScuOutputInit); // Aurora
    }

    private void OnScuOutputInit(Entity<ScuOutputComponent> ent, ref ComponentInit args) // Aurora
    {
        _scuOutput = ent;
    }

    private void UpdatePalletConsoleInterface(EntityUid uid, ContrabandPalletConsoleComponent comp)
    {
        var bui = _uiSystem.HasUi(uid, ContrabandPalletConsoleUiKey.Contraband);
        if (Transform(uid).GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(uid, ContrabandPalletConsoleUiKey.Contraband,
                new ContrabandPalletConsoleInterfaceState(0, 0, 0, false));
            return;
        }

        GetPalletGoods(gridUid, (uid, comp), out var toSell, out var amount, out var unregistered);

        var totalCount = toSell;
        toSell.UnionWith(unregistered);
        _uiSystem.SetUiState(uid, ContrabandPalletConsoleUiKey.Contraband,
            new ContrabandPalletConsoleInterfaceState((int) amount, totalCount.Count, unregistered.Count, true));
    }

    private void OnPalletUIOpen(EntityUid uid, ContrabandPalletConsoleComponent component, BoundUIOpenedEvent args)
    {
        var player = args.Actor;

        UpdatePalletConsoleInterface(uid, component);
    }

    /// <summary>
    /// Ok so this is just the same thing as opening the UI, its a refresh button.
    /// I know this would probably feel better if it were like predicted and dynamic as pallet contents change
    /// However.
    /// I dont want it to explode if cargo uses a conveyor to move 8000 pineapple slices or whatever, they are
    /// known for their entity spam i wouldnt put it past them
    /// </summary>

    private void OnPalletAppraise(EntityUid uid, ContrabandPalletConsoleComponent component, ContrabandPalletAppraiseMessage args)
    {
        var player = args.Actor;

        UpdatePalletConsoleInterface(uid, component);
    }

    private List<(EntityUid Entity, ContrabandPalletComponent Component)> GetContrabandPallets(EntityUid consoleUid, EntityUid gridUid) // Aurora copy over max distance limit
    {
        var pads = new List<(EntityUid, ContrabandPalletComponent)>();

        if (!TryComp(consoleUid, out TransformComponent? consoleXform))
            return pads;

        var query = AllEntityQuery<ContrabandPalletComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var compXform))
        {
            // Short-path easy checks
            if (compXform.ParentUid != gridUid
                || !compXform.Anchored)
            {
                continue;
            }

            // Check distance on pallets
            var distance = CalculateDistance(compXform.Coordinates, consoleXform.Coordinates);
            var maxPalletDistance = 8;

            // Get the mapped checking distance from the console
            if (TryComp<ContrabandPalletConsoleComponent>(consoleUid, out var cargoShuttleComponent))
                maxPalletDistance = cargoShuttleComponent.PalletDistance;

            if (distance > maxPalletDistance)
                continue;

            pads.Add((uid, comp));

        }

        return pads;
    }

    private void SellPallets(EntityUid gridUid, Entity<ContrabandPalletConsoleComponent> component, EntityUid? station, out int amount, EntityUid? actor = null)
    {
        station ??= _station.GetOwningStation(gridUid);
        GetPalletGoods(gridUid, component, out var toSell, out amount , out _);

        Log.Debug($"{component.Comp.Faction} sold {toSell.Count} contraband items for {amount}");

        // Aurora: Track sales for statistics if actor is provided
        if (actor != null && TryComp<MobStateComponent>(actor.Value, out var mobState))
        {
            var characterName = MetaData(actor.Value).EntityName;

            // Collect prototype IDs before deletion
            var itemPrototypes = toSell
                .Select(item => MetaData(item).EntityPrototype?.ID)
                .Where(id => !string.IsNullOrEmpty(id))
                .Cast<string>()
                .ToList();

            if (itemPrototypes.Count > 0)
            {
                var saleEvent = new ContrabandSaleEvent(
                    actor.Value,
                    characterName,
                    itemPrototypes,
                    amount,
                    component.Owner
                );
                RaiseLocalEvent(ref saleEvent);
            }
        }

        if (station != null)
        {
            var ev = new NFEntitySoldEvent(toSell, gridUid);
            RaiseLocalEvent(ref ev);
        }

        foreach (var ent in toSell)
        {
            Del(ent);
        }
    }

    private void GetPalletGoods(EntityUid gridUid, Entity<ContrabandPalletConsoleComponent> console, out HashSet<EntityUid> toSell, out int amount, out HashSet<EntityUid> unregistered)
    {
        amount = 0;
        toSell = new HashSet<EntityUid>();
        unregistered = new HashSet<EntityUid>(); // Aurora

        foreach (var (palletUid, _) in GetContrabandPallets(console, gridUid))
        {
            foreach (var ent in _lookup.GetEntitiesIntersecting(palletUid,
                         LookupFlags.Dynamic | LookupFlags.Sundries | LookupFlags.Approximate))
            {
                // Dont sell:
                // - anything already being sold
                // - anything anchored (e.g. light fixtures)
                // - anything blacklisted (e.g. players).
                if (_xformQuery.TryGetComponent(ent, out var xform) &&
                    (xform.Anchored || !CanSell(ent, xform)))
                {
                    continue;
                }

                if (_blacklistQuery.HasComponent(ent))
                    continue;

                if (TryComp<ContrabandComponent>(ent, out var comp)
                    && !toSell.Contains(ent)
                    && comp.TurnInValues is { } turnInValues
                    && turnInValues.ContainsKey(console.Comp.RewardType))
                {
                    toSell.Add(ent);
                    var value = comp.TurnInValues[console.Comp.RewardType];
                    amount += value;
                }

                // Aurora
                if (MetaData(ent).EntityPrototype is {} proto
                    && console.Comp.RegisterRecipies.ContainsKey(proto))
                {
                    unregistered.Add(ent);
                }
            }
        }
    }

    private bool CanSell(EntityUid uid, TransformComponent xform)
    {
        if (_mobQuery.HasComponent(uid))
        {
            if (_mobQuery.GetComponent(uid).CurrentState == MobState.Dead) // Allow selling alive prisoners
            {
                return false;
            }
            return true;
        }

        // Recursively check for mobs at any point.
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (!CanSell(child, _xformQuery.GetComponent(child)))
                return false;
        }
        // Look for blacklisted items and stop the selling of the container.
        if (_blacklistQuery.HasComponent(uid))
        {
            return false;
        }
        return true;
    }

    private void OnPalletSale(EntityUid uid, ContrabandPalletConsoleComponent component, ContrabandPalletSellMessage args)
    {
        var player = args.Actor;

        if (!CheckLicense(component, player)) // Aurora: add check for chl
        {
            PlayDenyEffect((uid,component));
            return;
        }

        if (Transform(uid).GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(uid, ContrabandPalletConsoleUiKey.Contraband,
                new ContrabandPalletConsoleInterfaceState(0, 0, 0, false));
            return;
        }

        SellPallets(gridUid, (uid, component), null, out var price, args.Actor);

        var stackPrototype = _protoMan.Index<StackPrototype>(component.RewardType);
        var stackUid = _stack.Spawn(price, stackPrototype, _scuOutput.ToCoordinates()); // Aurora spawn on scu output
        _transform.SetLocalRotation(stackUid, Angle.Zero); // Orient these to grid north instead of map north

        var rewardPrototype = _protoMan.Index<StackPrototype>(component.RewardCashPrototype); // Aurora: need EC prototype defined in scope
        stackUid = _stack.Spawn(price, rewardPrototype, args.Actor.ToCoordinates()); // Aurora: spawn "cash" (now EC)
        if (!_hands.TryPickupAnyHand(args.Actor, stackUid))
            _transform.SetLocalRotation(stackUid, Angle.Zero); // Orient these to grid north instead of map north
        UpdatePalletConsoleInterface(uid, component);
    }

    // Aurora - Contra registering
    private void OnPalletRegister(Entity<ContrabandPalletConsoleComponent> ent, ref ContrabandPalletRegisterMessage args)
    {
        if (!CheckLicense(ent.Comp, args.Actor)) // check for chl
        {
            PlayDenyEffect(ent);
            return;
        }

        if (Transform(ent).GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(ent.Owner, ContrabandPalletConsoleUiKey.Contraband,
                new ContrabandPalletConsoleInterfaceState(0, 0, 0, false));
            return;
        }
        GetPalletGoods(gridUid, ent, out _, out _ , out var toRegister);

        // Aurora: Collect prototype IDs before deletion for statistics tracking
        var itemPrototypes = toRegister
            .Select(item => MetaData(item).EntityPrototype?.ID)
            .Where(id => !string.IsNullOrEmpty(id))
            .Cast<string>()
            .ToList();

        // Aurora Song modification: Collect firearm serial numbers with their prototype IDs
        var serialNumbers = new Dictionary<string, string>();
        foreach (var item in toRegister)
        {
            if (TryComp<Content.Shared._AS.Weapons.Ranged.Components.FirearmSerialNumberComponent>(item, out var serialComp)
                && !string.IsNullOrEmpty(serialComp.SerialNumber))
            {
                var protoId = MetaData(item).EntityPrototype?.ID ?? string.Empty;
                if (!string.IsNullOrEmpty(protoId))
                {
                    serialNumbers[serialComp.SerialNumber] = protoId;
                }
            }
        }
        // End Aurora Song modification

        // Award SCUs
        var stackPrototype = _protoMan.Index<StackPrototype>(ent.Comp.RewardType);
        // 1 SCU per registered item
        if (Exists(_scuOutput))
        {
            var stackUid = _stack.Spawn(toRegister.Count, stackPrototype, Transform(_scuOutput).Coordinates);
            _transform.SetLocalRotation(stackUid, Angle.Zero); // Orient these to grid north instead of map north
        }

        //Exchange each item for their registered counterpart
        foreach (var oldEnt in toRegister)
        {
            if (MetaData(oldEnt).EntityPrototype is not {} oldProto)
                continue;
            ent.Comp.RegisterRecipies.TryGetValue(oldProto, out var newProto);
            var newEnt = SpawnAtPosition(newProto, Transform(oldEnt).Coordinates);
            _transform.SetLocalRotation(newEnt, Angle.Zero);

            // Aurora Song: Transfer serial number for firearms
            if (TryComp<Content.Shared._AS.Weapons.Ranged.Components.FirearmSerialNumberComponent>(oldEnt, out var oldSerial)
                && TryComp<Content.Shared._AS.Weapons.Ranged.Components.FirearmSerialNumberComponent>(newEnt, out var newSerial))
            {
                if (!string.IsNullOrEmpty(oldSerial.SerialNumber))
                {
                    newSerial.SerialNumber = oldSerial.SerialNumber;
                    newSerial.SerialFiled = oldSerial.SerialFiled;
                    Dirty(newEnt, newSerial);
                }
            }
            // End Aurora Song

            // Transfer items into new ent
            if (TryComp<ContainerManagerComponent>(oldEnt, out var oldManager)
                && TryComp<ContainerManagerComponent>(newEnt, out var newManager))
            {
                foreach (var newContainer in newManager.Containers)
                {
                    if (newContainer.Key == "actions" || newContainer.Key == "toggleable-clothing")
                        continue;
                    if (!oldManager.Containers.TryGetValue(newContainer.Key, out var oldContainer))
                        continue;
                    _container.CleanContainer(newContainer.Value);
                    var entsToTransfer = oldContainer.ContainedEntities;
                    foreach (var item in entsToTransfer)
                    {
                        _container.Insert(item, newContainer.Value);
                    }
                }
            }

            Del(oldEnt);
            Log.Debug($"{ent.Comp.Faction} registered {oldEnt} into {newEnt}");
        }

        // Aurora: Raise statistics event for registered items
        if (itemPrototypes.Count > 0)
        {
            var characterName = MetaData(args.Actor).EntityName;
            var registrationEvent = new ContrabandRegistrationEvent(
                args.Actor,
                characterName,
                itemPrototypes,
                ent.Owner,
                serialNumbers // Aurora Song modification
            );
            RaiseLocalEvent(ref registrationEvent);
        }

        UpdatePalletConsoleInterface(ent, ent.Comp);
    }

    private bool CheckLicense(ContrabandPalletConsoleComponent console, EntityUid user) // Aurora
    {
        if (console.LicenseRequired == null)
            return true;
        if (!_inventory.TryGetSlotEntity(user, "id", out var slotEnt))
            return false;
        if (TryComp<LicenseComponent>(slotEnt, out var license) && license.LicenseName == console.LicenseRequired)
            return true;
        if (!_container.TryGetContainer(slotEnt.Value, "PDA-license", out var container))
            return false;
        foreach (var containerEnt in container.ContainedEntities)
        {
            if (TryComp<LicenseComponent>(containerEnt, out license) && license.LicenseName == console.LicenseRequired)
                return true;
        }
        foreach (var heldEnt in _hands.EnumerateHeld(user))
        {
            if (TryComp<LicenseComponent>(heldEnt, out license) && license.LicenseName == console.LicenseRequired)
                return true;
        }
        return false;
    }

    public void PlayDenyEffect(Entity<ContrabandPalletConsoleComponent> target)
    {
        _popup.PopupCoordinates(Loc.GetString("chl-required"), Transform(target).Coordinates);
        _audio.PlayPvs(_audio.ResolveSound(target.Comp.ErrorSound), target);
    }

    // Aurora - copied from NFCargoSystem
    /// <summary>
    /// Calculates distance between two EntityCoordinates
    /// Used to check for cargo pallets around the console instead of on the grid.
    /// </summary>
    /// <param name="point1">first point to get distance between</param>
    /// <param name="point2">second point to get distance between</param>
    /// <returns></returns>
    public static double CalculateDistance(EntityCoordinates point1, EntityCoordinates point2)
    {
        var xDifference = point2.X - point1.X;
        var yDifference = point2.Y - point1.Y;

        return Math.Sqrt(xDifference * xDifference + yDifference * yDifference);
    }
}
