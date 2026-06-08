using Content.Shared._Floof.Consent;
using Content.Shared.EntityConditions;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
using Content.Shared.Mind.Components;

namespace Content.Shared._AS.Consent.EntityEffects;

public sealed class ConsentEntityConditionSystem
    : EntityConditionSystem<MindContainerComponent, Consent>
{
    [Dependency] private readonly SharedConsentSystem _consent = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    protected override void Condition(Entity<MindContainerComponent> ent, ref EntityConditionEvent<Consent> args)
    {
        args.Result = false;

        if (!_mind.TryGetMind(ent.Owner, out _, out var mind))
            return;

        if (mind.Session is not { } session)
            return;

        if (!_consent.TryGetConsent(session.UserId, out var settings))
            return;

        foreach (var effect in args.Condition.EffectTypes)
        {
            if (_consent.HasConsent(settings, effect))
            {
                args.Result = true;
                return;
            }
        }
    }
}

public sealed partial class Consent : EntityConditionBase<Consent>
{
    [DataField(required: true)]
    public List<ProtoId<ConsentTogglePrototype>> EffectTypes;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
