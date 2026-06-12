using Content.Server.Power.Components;
using Content.Shared._EE.Silicon.Systems;
using Content.Shared.Bed.Sleep;
using Content.Server._EE.Silicon.Charge;
using Content.Server._EE.Power.Components;
using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.Inventory; // Aurora's Song
using Content.Shared.Mobs; // Aurora's Song
using Content.Shared.Power.Components;
using Content.Shared.StatusEffectNew; // starcup

namespace Content.Server._EE.Silicon.Death;

public sealed class SiliconDeathSystem : EntitySystem
{
    [Dependency] private readonly SleepingSystem _sleep = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly HideableHumanoidLayersSystem _hidableLayers = default!; // Aurora's Song
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!; // starcup

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconDownOnDeadComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);
        SubscribeLocalEvent<SiliconDownOnDeadComponent, MobStateChangedEvent>(OnSiliconMobStateChange); // Aurora's Song
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, SiliconChargeStateUpdateEvent args)
    {
        if (!_silicon.TryGetSiliconBattery(uid, out var battery)) // starcup
        {
            SiliconDead(uid, siliconDeadComp, (uid, null)); // starcup
            return;
        }

        if (args.ChargePercent == 0 && siliconDeadComp.Dead)
            return;

        if (args.ChargePercent == 0 && !siliconDeadComp.Dead)
            SiliconDead(uid, siliconDeadComp, battery.Value.AsNullable()); // starcup
        else if (args.ChargePercent != 0 && siliconDeadComp.Dead)
                SiliconUnDead(uid, siliconDeadComp, battery.Value.AsNullable()); // starcup
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, Entity<BatteryComponent?> battery) // starcup
    {
        var deadEvent = new SiliconChargeDyingEvent(uid, battery); // starcup
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        EntityManager.EnsureComponent<SleepingComponent>(uid);
        _statusEffect.TrySetStatusEffectDuration(uid, SleepingSystem.StatusEffectForcedSleeping); // starcup: edited for status effects refactor

        _hidableLayers.SetLayerOcclusion(uid, HumanoidVisualLayers.Eyes, hidden: true, SlotFlags.PREVENTEQUIP); // Aurora's Song

        siliconDeadComp.Dead = true;

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, battery)); // starcup
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, Entity<BatteryComponent?> battery) // starcup
    {
        _statusEffect.TryRemoveStatusEffect(uid, SleepingSystem.StatusEffectForcedSleeping); // starcup: edited for status effects refactor
        _sleep.TryWaking(uid, true, null);

        _hidableLayers.SetLayerOcclusion(uid, HumanoidVisualLayers.Eyes, hidden: false, SlotFlags.PREVENTEQUIP); // Aurora's Song

        siliconDeadComp.Dead = false;

        RaiseLocalEvent(uid, new SiliconChargeAliveEvent(uid, battery)); // starcup
    }

    // Aurora's Song - Make them turn off their screen on actual death
    private void OnSiliconMobStateChange(EntityUid uid, SiliconDownOnDeadComponent component, MobStateChangedEvent args)
    {
        _hidableLayers.SetLayerOcclusion(uid, HumanoidVisualLayers.Eyes, hidden: args.NewMobState != MobState.Alive, SlotFlags.PREVENTEQUIP);
    }
}

/// <summary>
///     A cancellable event raised when a Silicon is about to go down due to charge.
/// </summary>
/// <remarks>
///     This probably shouldn't be modified unless you intend to fill the Silicon's battery,
///     as otherwise it'll just be triggered again next frame.
/// </remarks>
public sealed class SiliconChargeDyingEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDyingEvent(EntityUid siliconUid, Entity<BatteryComponent?> battery) // starcup
    {
        SiliconUid = siliconUid;
        BatteryComp = battery.Comp; // starcup
        BatteryUid = battery.Owner; // starcup
    }
}

/// <summary>
///     An event raised after a Silicon has gone down due to charge.
/// </summary>
public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, Entity<BatteryComponent?> battery) // starcup
    {
        SiliconUid = siliconUid;
        BatteryComp = battery.Comp; // starcup
        BatteryUid = battery.Owner; // starcup
    }
}

/// <summary>
///     An event raised after a Silicon has reawoken due to an increase in charge.
/// </summary>
public sealed class SiliconChargeAliveEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeAliveEvent(EntityUid siliconUid, Entity<BatteryComponent?> battery) // starcup
    {
        SiliconUid = siliconUid;
        BatteryComp = battery.Comp; // starcup
        BatteryUid = battery.Owner; // starcup
    }
}
