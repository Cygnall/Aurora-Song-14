using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Item.ItemToggle; // Goobstation
//using Content.Shared.Maps; // Impstation - Unused after blocking rework
//using Content.Shared.Mobs.Components; // Impstation - Unused after blocking rework
//using Content.Shared.Physics; // Impstation - Unused after blocking rework
using Content.Shared.Movement.Systems; // Impstation
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Verbs;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
//using Robust.Shared.Physics.Systems; // Impstation - Unused after blocking rework
using Robust.Shared.Player; // Impstation
using Robust.Shared.Timing; // Impstation
using Robust.Shared.Utility;

namespace Content.Shared.Blocking;

public sealed partial class BlockingSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    //[Dependency] private SharedTransformSystem _transformSystem = default!;  // Impstation - Unused after blocking rework
    //[Dependency] private FixtureSystem _fixtureSystem = default!; // Impstation - Unused after blocking rework
    [Dependency] private SharedHandsSystem _handsSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    //[Dependency] private EntityLookupSystem _lookup = default!;  // Impstation - Unused after blocking rework
    //[Dependency] private SharedPhysicsSystem _physics = default!; // Impstation - Unused after blocking rework
    [Dependency] private ExamineSystemShared _examine = default!;
    //[Dependency] private TurfSystem _turf = default!; // Impstation - Unused after blocking rework
    [Dependency] private ItemToggleSystem _toggle = default!; // Goobstation

    [Dependency] private EntityQuery<BlockingComponent> _blockQuery = default!;
    [Dependency] private EntityQuery<HandsComponent> _handQuery = default!;
    //[Dependency] private EntityQuery<MobStateComponent> _mobQuery = default!; // Impstation - Unused after blocking rework
    [Dependency] private EntityQuery<BlockingUserComponent> _userQuery = default!;
    [Dependency] private IGameTiming _gameTiming = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUser();

        SubscribeLocalEvent<BlockingComponent, GotEquippedHandEvent>(OnEquip);
        SubscribeLocalEvent<BlockingComponent, GotUnequippedHandEvent>(OnUnequip);
        SubscribeLocalEvent<BlockingComponent, DroppedEvent>(OnDrop);

        SubscribeLocalEvent<BlockingComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<BlockingComponent, ToggleActionEvent>(OnToggleAction);

        SubscribeLocalEvent<BlockingComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<BlockingComponent, GetVerbsEvent<ExamineVerb>>(OnVerbExamine);
        SubscribeLocalEvent<BlockingComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<BlockingUserComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed); // imp
    }

    private void OnMapInit(EntityUid uid, BlockingComponent component, MapInitEvent args)
    {
        _actionContainer.EnsureAction(uid, ref component.BlockingToggleActionEntity, component.BlockingToggleAction);
        Dirty(uid, component);
    }

    private void OnEquip(EntityUid uid, BlockingComponent component, GotEquippedHandEvent args)
    {
        component.User = args.User;
        Dirty(uid, component);

        //To make sure that this bodytype doesn't get set as anything but the original
        if (TryComp<PhysicsComponent>(args.User, out var physicsComponent) && physicsComponent.BodyType != BodyType.Static && !HasComp<BlockingUserComponent>(args.User))
        {
            var userComp = EnsureComp<BlockingUserComponent>(args.User);
            userComp.BlockingItem = uid;
            userComp.OriginalBodyType = physicsComponent.BodyType;
        }
    }

    private void OnUnequip(EntityUid uid, BlockingComponent component, GotUnequippedHandEvent args)
    {
        StopBlockingHelper(uid, component, args.User);
    }

    private void OnDrop(EntityUid uid, BlockingComponent component, DroppedEvent args)
    {
        StopBlockingHelper(uid, component, args.User);
    }

    private void OnGetActions(EntityUid uid, BlockingComponent component, GetItemActionsEvent args)
    {
        args.AddAction(ref component.BlockingToggleActionEntity, component.BlockingToggleAction);
    }

    private void OnToggleAction(EntityUid uid, BlockingComponent component, ToggleActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_handQuery.TryGetComponent(args.Performer, out var hands))
            return;

        var shields = _handsSystem.EnumerateHeld((args.Performer, hands)).ToArray();

        foreach (var shield in shields)
        {
            if (shield == uid)
                continue;

            if (_blockQuery.TryGetComponent(shield, out var otherBlockComp) && otherBlockComp.IsBlocking)
            {
                CantBlockError(args.Performer);
                return;
            }
        }

        if (component.IsBlocking)
            StopBlocking((uid, component), args.Performer); // imp. changed to Entity<T>
        else
            StartBlocking((uid, component), args.Performer); // imp. ditto

        args.Handled = true;
    }

    private void OnShutdown(EntityUid uid, BlockingComponent component, ComponentShutdown args)
    {
        //In theory the user should not be null when this fires off
        if (component.User != null)
        {
            _actionsSystem.RemoveProvidedActions(component.User.Value, uid);
            StopBlockingHelper(uid, component, component.User.Value);
        }
    }

    /// <summary>
    /// Called when the user starts blocking.
    /// </summary>
    /// <param name="ent">The entity with the blocking component.</param>
    /// <param name="user">The entity who's using the item to block.</param>
    // imp - redid this whole thing to remove the anchoring part and replace it with a movement speed modifier.
    public void StartBlocking(Entity<BlockingComponent> ent, EntityUid user)
    {
        if (ent.Comp.IsBlocking)
            return;

        var shieldName = Name(ent);

        var blockerName = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-blocking-user", ("shield", shieldName));
        var msgOther = Loc.GetString("action-popup-blocking-other", ("blockerName", blockerName), ("shield", shieldName));

        // Don't allow someone to block if they're not holding the shield
        if (!_handsSystem.IsHolding(user, ent, out _))
        {
            CantBlockError(user);
            return;
        }

        _actionsSystem.SetToggled(ent.Comp.BlockingToggleActionEntity, true);
        if (_gameTiming.IsFirstTimePredicted)
        {
            _popupSystem.PopupEntity(msgOther, user, Filter.PvsExcept(user), true);
            if (_gameTiming.InPrediction)
                _popupSystem.PopupEntity(msgUser, user, user);
        }

        ent.Comp.IsBlocking = true;
        Dirty(ent);

        _movementSpeed.RefreshMovementSpeedModifiers(user);

        return;

        // Original StartBlocking

        /*
        public bool StartBlocking(EntityUid item, BlockingComponent component, EntityUid user)
        {
        if (component.IsBlocking)
            return false;

        var xform = Transform(user);

        var shieldName = Name(item);

        var blockerName = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-blocking-user", ("shield", shieldName));
        var msgOther = Loc.GetString("action-popup-blocking-other", ("blockerName", blockerName), ("shield", shieldName));

        //Don't allow someone to block if they're not parented to a grid
        if (xform.GridUid != xform.ParentUid)
        {
            CantBlockError(user);
            return false;
        }

        // Don't allow someone to block if they're not holding the shield
        if (!_handsSystem.IsHolding(user, item, out _))
        {
            CantBlockError(user);
            return false;
        }

        //Don't allow someone to block if someone else is on the same tile
        var playerTileRef = _turf.GetTileRef(xform.Coordinates);
        if (playerTileRef != null)
        {
            var intersecting = _lookup.GetLocalEntitiesIntersecting(playerTileRef.Value, 0f);
            foreach (var uid in intersecting)
            {
                if (uid != user && _mobQuery.HasComponent(uid))
                {
                    TooCloseError(user);
                    return false;
                }
            }
        }

        //Don't allow someone to block if they're somehow not anchored.
        _transformSystem.AnchorEntity(user, xform);
        if (!xform.Anchored)
        {
            CantBlockError(user);
            return false;
        }
        _actionsSystem.SetToggled(component.BlockingToggleActionEntity, true);
        _popupSystem.PopupPredicted(msgUser, msgOther, user, user);

        if (TryComp<PhysicsComponent>(user, out var physicsComponent))
        {
            _fixtureSystem.TryCreateFixture(user,
                component.Shape,
                BlockingComponent.BlockFixtureID,
                hard: false, // Frontier: true<false, mobs AI abuse.
                collisionLayer: (int)CollisionGroup.WallLayer,
                body: physicsComponent);
        }

        component.IsBlocking = true;
        Dirty(item, component);

        return true;
        }
        */
    }

    private void CantBlockError(EntityUid user)
    {
        var msgError = Loc.GetString("action-popup-blocking-user-cant-block");
        _popupSystem.PopupClient(msgError, user, user);
    }

    /// <summary>
    /// Called when the user stops blocking.
    /// </summary>
    /// <param name="ent">The entity with the blocking component.</param>
    /// <param name="user">The entity who's using the item to block.</param>
    // imp - changed this whole thing to remove fixtures/anchoring and replace with slowdown
    public void StopBlocking(Entity<BlockingComponent> ent, EntityUid user)
    {
        if (!ent.Comp.IsBlocking)
            return;

        var shieldName = Name(ent);

        var blockerName = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-blocking-disabling-user", ("shield", shieldName));
        var msgOther = Loc.GetString("action-popup-blocking-disabling-other", ("blockerName", blockerName), ("shield", shieldName));

        if (TryComp<BlockingUserComponent>(user, out _))
        {
            _actionsSystem.SetToggled(ent.Comp.BlockingToggleActionEntity, false);
            if (_gameTiming.IsFirstTimePredicted)
            {
                _popupSystem.PopupEntity(msgOther, user, Filter.PvsExcept(user), true);
                if (_gameTiming.InPrediction)
                    _popupSystem.PopupEntity(msgUser, user, user);
            }
        }

        ent.Comp.IsBlocking = false;
        Dirty(ent);

        _movementSpeed.RefreshMovementSpeedModifiers(user);

        return;

        // Original StopBlocking

        /*
        public bool StopBlocking(EntityUid item, BlockingComponent component, EntityUid user)
        {
        if (!component.IsBlocking)
            return false;

        var xform = Transform(user);

        var shieldName = Name(item);

        var blockerName = Identity.Entity(user, EntityManager);
        var msgUser = Loc.GetString("action-popup-blocking-disabling-user", ("shield", shieldName));
        var msgOther = Loc.GetString("action-popup-blocking-disabling-other", ("blockerName", blockerName), ("shield", shieldName));

        //If the component blocking toggle isn't null, grab the users SharedBlockingUserComponent and PhysicsComponent
        //then toggle the action to false, unanchor the user, remove the hard fixture
        //and set the users bodytype back to their original type
        if (TryComp<BlockingUserComponent>(user, out var blockingUserComponent) && TryComp<PhysicsComponent>(user, out var physicsComponent))
        {
            if (xform.Anchored)
                _transformSystem.Unanchor(user, xform, false);

            _actionsSystem.SetToggled(component.BlockingToggleActionEntity, false);
            _fixtureSystem.DestroyFixture(user, BlockingComponent.BlockFixtureID, body: physicsComponent);
            _physics.SetBodyType(user, blockingUserComponent.OriginalBodyType, body: physicsComponent);
            _popupSystem.PopupPredicted(msgUser, msgOther, user, user);
        }

        component.IsBlocking = false;
        Dirty(item, component);

        return true;
        }
        */
    }

    // imp add - necessary for movement speed modifier
    private void OnRefreshMovespeed(Entity<BlockingUserComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<BlockingComponent>(ent.Comp.BlockingItem, out var blockingComp))
            return;

        if (blockingComp.IsBlocking)
            args.ModifySpeed(blockingComp.SlowdownModifier);
        else
            args.ModifySpeed(1f);
    }

    /// <summary>
    /// Called where you want someone to stop blocking and to remove the <see cref="BlockingUserComponent"/> from them
    /// Won't remove the <see cref="BlockingUserComponent"/> if they're holding another blocking item
    /// </summary>
    /// <param name="uid"> The item the component is attached to</param>
    /// <param name="component"> The <see cref="BlockingComponent"/> </param>
    /// <param name="user"> The person holding the blocking item </param>
    private void StopBlockingHelper(EntityUid uid, BlockingComponent component, EntityUid user)
    {
        if (component.IsBlocking)
            StopBlocking((uid, component), user); // imp - switched to Entity<T>

        if (!_handQuery.TryGetComponent(user, out var hands))
            return;

        var shields = _handsSystem.EnumerateHeld((user, hands)).ToArray();

        foreach (var shield in shields)
        {
            if (HasComp<BlockingComponent>(shield) && _userQuery.TryGetComponent(user, out var blockingUserComponent))
            {
                blockingUserComponent.BlockingItem = shield;
                return;
            }
        }

        RemComp<BlockingUserComponent>(user);
        component.User = null;
    }

    private void OnVerbExamine(EntityUid uid, BlockingComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var fraction = component.IsBlocking ? component.ActiveBlockFraction : component.PassiveBlockFraction;
        if (!_toggle.IsActivated(uid)) // Goobstation
            fraction = 0f;
        var modifier = component.IsBlocking ? component.ActiveBlockDamageModifier : component.PassiveBlockDamageModifer;

        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("blocking-fraction", ("value", MathF.Round(fraction * 100, 1))));

        AppendCoefficients(modifier, msg);

        _examine.AddDetailedExamineVerb(args, component, msg,
            Loc.GetString("blocking-examinable-verb-text"),
            "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("blocking-examinable-verb-message")
        );
    }

    private void AppendCoefficients(DamageModifierSet modifiers, FormattedMessage msg)
    {
        foreach (var coefficient in modifiers.Coefficients)
        {
            msg.PushNewline();
            msg.AddMarkupOrThrow(Robust.Shared.Localization.Loc.GetString("blocking-coefficient-value",
                ("type", coefficient.Key),
                ("value", MathF.Round(coefficient.Value * 100, 1))
            ));
        }

        foreach (var flat in modifiers.FlatReduction)
        {
            msg.PushNewline();
            msg.AddMarkupOrThrow(Robust.Shared.Localization.Loc.GetString("blocking-reduction-value",
                ("type", flat.Key),
                ("value", flat.Value)
            ));
        }
    }
}
