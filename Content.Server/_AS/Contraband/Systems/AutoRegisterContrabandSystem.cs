using Content.Shared._AS.Contraband.Components;
using Content.Shared._AS.Contraband.Events;
using Content.Shared._AS.Weapons.Ranged.Components;
using Content.Shared.Contraband;
using Content.Shared.IdentityManagement;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Log;

namespace Content.Server._AS.Contraband.Systems;

public sealed class AutoRegisterContrabandSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Dictionary<EntityUid, EntityUid> _pendingStorePurchases = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ContrabandComponent, MapInitEvent>(OnContrabandMapInit);
        SubscribeLocalEvent<AutoRegisterContrabandComponent, StoreBuyListingMessage>(OnStoreBuyListing, before: [typeof(Content.Server.Store.Systems.StoreSystem)]);
    }

    private void OnStoreBuyListing(Entity<AutoRegisterContrabandComponent> store, ref StoreBuyListingMessage args)
    {
        // Track the buyer for the upcoming spawn
        store.Comp.LastBuyer = args.Actor;
        store.Comp.LastBuyTime = _timing.CurTime;
        _pendingStorePurchases[store] = args.Actor;
    }

    private void OnContrabandMapInit(Entity<ContrabandComponent> ent, ref MapInitEvent args)
    {
        Log.Debug($"AutoRegister: Contraband MapInit for {ToPrettyString(ent)} at {_timing.CurTime}");
        EntityUid? buyer = null;
        EntityUid? sourceEntity = null;

        // Check all entities with AutoRegisterContrabandComponent for a recent purchase
        // This works for both vending machines and stores/uplinks since items spawn at coordinates
        var query = EntityQueryEnumerator<AutoRegisterContrabandComponent>();
        while (query.MoveNext(out var uid, out var autoReg))
        {
            Log.Debug($"AutoRegister: Checking {ToPrettyString(uid)} - LastBuyer={autoReg.LastBuyer}, LastBuyTime={autoReg.LastBuyTime}, TimeDiff={_timing.CurTime - autoReg.LastBuyTime}");
            // Check if this was a recent purchase (within 5 seconds to account for spawn timing)
            if (autoReg.LastBuyer != null && _timing.CurTime - autoReg.LastBuyTime <= TimeSpan.FromSeconds(5))
            {
                buyer = autoReg.LastBuyer.Value;
                sourceEntity = uid;
                Log.Debug($"AutoRegister: Found recent buyer! buyer={buyer}, source={ToPrettyString(uid)}");
                break;
            }
        }

        // Clean up old pending store purchases
        var toRemove = new List<EntityUid>();
        foreach (var uid in _pendingStorePurchases.Keys)
        {
            if (!Exists(uid))
                toRemove.Add(uid);
        }
        foreach (var uid in toRemove)
            _pendingStorePurchases.Remove(uid);

        if (buyer == null || sourceEntity == null)
            return;

        // Check if it's Class 1 or Class 2 contraband
        var severity = _proto.Index(ent.Comp.Severity);
        if (severity.ID != "Class1" && severity.ID != "Class2")
            return;

        // Filter out ammunition (cartridges) and magazines/clips, but NOT firearms
        // Firearms have serial numbers, so check for that first
        var isFirearm = HasComp<FirearmSerialNumberComponent>(ent);
        if (!isFirearm && (HasComp<Content.Shared.Weapons.Ranged.Components.CartridgeAmmoComponent>(ent) ||
            HasComp<Content.Shared.Weapons.Ranged.Components.BallisticAmmoProviderComponent>(ent)))
        {
            Log.Debug($"AutoRegister: Skipping ammunition/magazine {ToPrettyString(ent)}");
            return;
        }

        // Get character name
        var characterName = Identity.Name(buyer.Value, EntityManager);

        // Get prototype ID
        var protoId = MetaData(ent).EntityPrototype?.ID ?? string.Empty;

        // Collect serial numbers if it's a firearm (map serial -> prototype ID)
        var serialNumbers = new Dictionary<string, string>();
        if (TryComp<FirearmSerialNumberComponent>(ent, out var serialComp) && !string.IsNullOrEmpty(serialComp.SerialNumber))
        {
            serialNumbers[serialComp.SerialNumber] = protoId;
        }

        // Raise registration event
        var registrationEvent = new ContrabandRegistrationEvent(
            Actor: buyer.Value,
            CharacterName: characterName,
            ItemPrototypeIds: new List<string> { protoId },
            Console: sourceEntity.Value,
            FirearmSerialNumbers: serialNumbers
        );

        RaiseLocalEvent(ref registrationEvent);
    }
}
