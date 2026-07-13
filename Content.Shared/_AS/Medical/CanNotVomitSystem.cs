using Content.Shared.Body;
using Content.Shared.Medical;

namespace Content.Shared._AS.Medical;

/// <summary>
/// Flags vomiting as canceled
/// </summary>
public sealed class CanNotVomitSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        // This needs to intercept the relay
        SubscribeLocalEvent<CanNotVomitComponent, TryVomitEvent>(OnVomit, before: [typeof(BodySystem)]);
    }

    private void OnVomit(Entity<CanNotVomitComponent> ent, ref TryVomitEvent args)
    {
        args.Cancelled = true;
    }
}
