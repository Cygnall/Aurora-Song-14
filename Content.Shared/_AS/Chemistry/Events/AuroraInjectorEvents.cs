using Content.Shared.Chemistry.Events;

namespace Content.Shared._AS.Chemistry.Events;

/// <summary>
///     This event is raised on the target before the injector is injected.
///     The event is used for species traits that block injection.
/// </summary>
[ByRefEvent]
public sealed class TargetBeforeInjectEventSpecies(EntityUid user, EntityUid usedInjector, EntityUid target, string? overrideMessage = null)
    : BeforeInjectTargetEvent(user, usedInjector, target, overrideMessage)
{
    public string BlockMessageSelf = "generic-injector-component-blocked-user";
    public string BlockMessageOther = "generic-injector-component-blocked-other";
}
