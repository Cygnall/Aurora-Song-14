using Content.Shared._AS.WeaponSwap;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._AS.WeaponSwap;

public sealed class WeaponSwapSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeaponSwapComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<WeaponSwapComponent, WeaponSwapDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, WeaponSwapComponent component, AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null)
            return;

        if (component.DoDistanceCheck && !args.CanReach)
            return;

        var target = args.Target.Value;

        // Check if the target is a gun
        if (!HasComp<GunComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("weapon-swap-not-a-gun"), uid, args.User);
            return;
        }

        // Get the prototype ID of the target weapon
        var proto = MetaData(target).EntityPrototype;
        if (proto == null)
        {
            _popup.PopupEntity(Loc.GetString("weapon-swap-no-prototype"), uid, args.User);
            return;
        }

        // Check if we have a mapping for this weapon
        if (!component.WeaponMappings.TryGetValue(proto.ID, out var variantProto))
        {
            _popup.PopupEntity(Loc.GetString("weapon-swap-no-variant", ("weapon", MetaData(target).EntityName)), uid, args.User);
            return;
        }

        // Verify the variant prototype exists
        if (!_proto.HasIndex<EntityPrototype>(variantProto))
        {
            _popup.PopupEntity(Loc.GetString("weapon-swap-invalid-variant"), uid, args.User);
            return;
        }

        // If confirmation is required, handle 2-click confirmation flow using popups & timeout
        if (component.RequireConfirmation)
        {
            var now = _timing.CurTime;

            // If no pending, or different target, or expired -> set pending and prompt
            var expired = component.PendingExpiry != null && now > component.PendingExpiry.Value;
            if (component.PendingTarget != target || expired)
            {
                component.PendingTarget = target;
                component.PendingExpiry = now + component.ConfirmationTimeout;
                _popup.PopupEntity(Loc.GetString("weapon-swap-confirm", ("weapon", MetaData(target).EntityName)), uid, args.User);
                return;
            }

            // Second click within timeout on same target -> proceed and clear pending
            component.PendingTarget = null;
            component.PendingExpiry = null;
        }

        // Play sound and start the swap process
        var audioStream = _audio.PlayPvs(component.SwapSound, uid);
        if (audioStream != null)
        {
            component.AudioStream = audioStream.Value.Entity;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, component.SwapDuration, new WeaponSwapDoAfterEvent(), uid, target: target, used: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true
        });

        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, WeaponSwapComponent component, DoAfterEvent args)
    {
        component.AudioStream = _audio.Stop(component.AudioStream);

        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        var target = args.Target.Value;

        // Double-check the target still exists and is valid
        if (!TryComp<GunComponent>(target, out _))
            return;

        var meta = MetaData(target);
        var proto = meta.EntityPrototype;
        if (proto == null)
            return;

        // Get the variant prototype
        if (!component.WeaponMappings.TryGetValue(proto.ID, out var variantProto))
            return;

        // Get the target's coordinates and container (if any)
        var xform = Transform(target);
        var coordinates = xform.Coordinates;
        ContainerSlot? parentContainer = null;

        if (xform.ParentUid.IsValid() && _container.TryGetContainingContainer((target, xform, meta), out var container))
        {
            if (container is ContainerSlot slot)
                parentContainer = slot;
        }

        // Store the name for feedback
        var oldName = meta.EntityName;

        // Delete the old weapon
        QueueDel(target);

        // Spawn the new variant
        var variant = Spawn(variantProto, coordinates);

        // Try to put it back in the same container/hand if applicable
        if (parentContainer != null)
        {
            _container.Insert(variant, parentContainer);
        }
        else
        {
            // Try to put it in the user's hand if they were holding it
            _hands.TryPickup(args.User, variant);
        }

        _popup.PopupEntity(Loc.GetString("weapon-swap-success", ("weapon", oldName)), uid, args.User);

        args.Handled = true;
    }
}
