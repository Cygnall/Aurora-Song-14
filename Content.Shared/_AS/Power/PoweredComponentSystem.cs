using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;

namespace Content.Shared.Power.Components;

/// <summary>
/// Handles <see cref="PoweredComponentComponent"/> component manipulation.
/// </summary>
public sealed partial class PoweredComponentSystem : EntitySystem
{
    [Dependency] private SharedPowerReceiverSystem _receiver = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PoweredComponentComponent, PowerChangedEvent>(OnPowerChanged);
    }
    private void OnPowerChanged(Entity<PoweredComponentComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered && !ent.Comp.Broken)
        {
            var target = ent.Comp.Parent ? Transform(ent).ParentUid : ent.Owner;

            if (TerminatingOrDeleted(target))
                return;

            ent.Comp.Target = target;

            EntityManager.AddComponents(target, ent.Comp.Components);
        }
        else
        {
            if (ent.Comp.Target == null)
                return;

            if (TerminatingOrDeleted(ent.Comp.Target.Value))
                return;

            EntityManager.RemoveComponents(ent.Comp.Target.Value, ent.Comp.RemoveComponents ?? ent.Comp.Components);
        }
    }
}
