using System.Linq;
using Content.Server._NF.Contraband.Components;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Stack;
using Content.Server.Station.Systems;
using Content.Shared._NF.Contraband;
using Content.Shared._NF.Contraband.BUI;
using Content.Shared._NF.Contraband.Components;
using Content.Shared._NF.Contraband.Events;
using Content.Shared.Contraband;
using Content.Shared.Stacks;
using Robust.Server.GameObjects;
using Content.Shared.Coordinates;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Content.Server._NF.Cargo.Systems;
using Content.Server.Hands.Systems;
using Content.Shared._AS.Contraband.Events;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Server._NF.Contraband.Systems;

/// <summary>
/// Contraband system. Contraband Pallet UI Console is mostly a copy of the system in cargo. Checkraze Note: copy of my code from cargosystems.shuttles.cs
/// </summary>
public sealed partial class ContrabandTurnInSystem : SharedContrabandTurnInSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!; // Aurora

    private EntityQuery<MobStateComponent> _mobQuery;
    private EntityQuery<TransformComponent> _xformQuery;
    private EntityQuery<CargoSellBlacklistComponent> _blacklistQuery;

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

        GetPalletGoods(gridUid, comp, out var toSell, out var amount, out var unregistered);

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

    private List<(EntityUid Entity, ContrabandPalletComponent Component)> GetContrabandPallets(EntityUid gridUid)
    {
        var pads = new List<(EntityUid, ContrabandPalletComponent)>();
        var query = AllEntityQuery<ContrabandPalletComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var compXform))
        {
            if (compXform.ParentUid != gridUid ||
                !compXform.Anchored)
            {
                continue;
            }

            pads.Add((uid, comp));
        }

        return pads;
    }

    private void SellPallets(EntityUid gridUid, ContrabandPalletConsoleComponent component, EntityUid? station, out int amount)
    {
        station ??= _station.GetOwningStation(gridUid);
        GetPalletGoods(gridUid, component, out var toSell, out amount , out _);

        Log.Debug($"{component.Faction} sold {toSell.Count} contraband items for {amount}");

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

    private void GetPalletGoods(EntityUid gridUid, ContrabandPalletConsoleComponent console, out HashSet<EntityUid> toSell, out int amount, out HashSet<EntityUid> unregistered)
    {
        amount = 0;
        toSell = new HashSet<EntityUid>();
        unregistered = new HashSet<EntityUid>(); // Aurora

        foreach (var (palletUid, _) in GetContrabandPallets(gridUid))
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
                    && turnInValues.ContainsKey(console.RewardType))
                {
                    toSell.Add(ent);
                    var value = comp.TurnInValues[console.RewardType];
                    amount += value;
                }

                // Aurora
                if (MetaData(ent).EntityPrototype is {} proto
                    && console.RegisterRecipies.ContainsKey(proto))
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

        if (Transform(uid).GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(uid, ContrabandPalletConsoleUiKey.Contraband,
                new ContrabandPalletConsoleInterfaceState(0, 0, 0, false));
            return;
        }

        SellPallets(gridUid, component, null, out var price);

        var stackPrototype = _protoMan.Index<StackPrototype>(component.RewardType);
        var stackUid = _stack.Spawn(price, stackPrototype, args.Actor.ToCoordinates());
        if (!_hands.TryPickupAnyHand(args.Actor, stackUid))
            _transform.SetLocalRotation(stackUid, Angle.Zero); // Orient these to grid north instead of map north
        UpdatePalletConsoleInterface(uid, component);
    }

    // Aurora - Contra registering
    private void OnPalletRegister(Entity<ContrabandPalletConsoleComponent> ent, ref ContrabandPalletRegisterMessage args)
    {
        if (Transform(ent).GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(ent.Owner, ContrabandPalletConsoleUiKey.Contraband,
                new ContrabandPalletConsoleInterfaceState(0, 0, 0, false));
            return;
        }
        GetPalletGoods(gridUid, ent.Comp, out _, out _ , out var toRegister);

        // Award SCUs
        var stackPrototype = _protoMan.Index<StackPrototype>(ent.Comp.RewardType);
        // 1 SCU per registered item
        var stackUid = _stack.Spawn(toRegister.Count, stackPrototype, args.Actor.ToCoordinates());
        if (!_hands.TryPickupAnyHand(args.Actor, stackUid))
            _transform.SetLocalRotation(stackUid, Angle.Zero); // Orient these to grid north instead of map north

        //Exchange each item for their registered counterpart
        foreach (var oldEnt in toRegister)
        {
            if (MetaData(oldEnt).EntityPrototype is not {} oldProto)
                continue;
            ent.Comp.RegisterRecipies.TryGetValue(oldProto, out var newProto);
            var newEnt = SpawnAtPosition(newProto, Transform(oldEnt).Coordinates);
            _transform.SetLocalRotation(newEnt, Angle.Zero);

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

        UpdatePalletConsoleInterface(ent, ent.Comp);
    }
}
