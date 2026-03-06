using Content.Shared._Floof.Consent;
using Content.Shared.EntityEffects;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Consent.EntityEffects;

public sealed partial class Consent : EntityEffectCondition
{
    [DataField(required: true)]
    public List<ProtoId<ConsentTogglePrototype>> EffectTypes = default!;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.System<SharedMindSystem>().TryGetMind(args.TargetEntity, out _, out var mind))
            return false;

        if (mind.Session is not { } session)
            return false;

        if (!args.EntityManager.System<SharedConsentSystem>().TryGetConsent(session.UserId, out var settings))
            return false;

        foreach (var effect in EffectTypes)
        {
            if (settings is not null && args.EntityManager.System<SharedConsentSystem>().HasConsent(settings, effect))
                return true;
        }
        return false;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
