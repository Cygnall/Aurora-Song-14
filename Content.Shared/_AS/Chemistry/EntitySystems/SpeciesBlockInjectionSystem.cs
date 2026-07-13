using Content.Shared._AS.Chemistry.Components;
using Content.Shared._AS.Chemistry.Events;

namespace Content.Shared._AS.Chemistry.EntitySystems;

public sealed class SpeciesBlockInjectionSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SpeciesBlockInjectionComponent, TargetBeforeInjectEventSpecies>(OnTargetBeforeInjectSpeciesEvent);
    }

    private void OnTargetBeforeInjectSpeciesEvent(Entity<SpeciesBlockInjectionComponent> entity, ref TargetBeforeInjectEventSpecies args)
    {
        args.BlockMessageSelf = entity.Comp.MessageSelf;
        args.BlockMessageOther = entity.Comp.MessageOther;
        args.Cancel();
    }
}
