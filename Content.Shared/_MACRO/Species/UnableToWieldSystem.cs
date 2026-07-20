using Content.Shared.Popups;
using Content.Shared.Wieldable;

namespace Content.Shared._MACRO.UnableToWield;

public sealed partial class UnableToWieldSystem : EntitySystem // Aurora's Song - Make partial
{
    [Dependency] private SharedPopupSystem _popup = default!; // Aurora's Song - Remove readonly

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnableToWieldComponent, WieldAttemptEvent>(OnWieldAttempt);
    }

    private void OnWieldAttempt(Entity<UnableToWieldComponent> ent, ref WieldAttemptEvent args)
    {
        args.Cancel();

        if (ent.Comp.PopupText != null)
            _popup.PopupClient(Loc.GetString(ent.Comp.PopupText), ent, ent);
    }
}
