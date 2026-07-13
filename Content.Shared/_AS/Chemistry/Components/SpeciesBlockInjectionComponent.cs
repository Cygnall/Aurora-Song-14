using Content.Shared._AS.Chemistry.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._AS.Chemistry.Components;

/// <summary>
/// Used on species to block injection separately from (currently unimplemented) injection-blocking clothes.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SpeciesBlockInjectionSystem))]
public sealed partial class SpeciesBlockInjectionComponent : Component
{

    /// <summary>
    /// The message to popup when injection fails on the user
    /// </summary>
    [DataField]
    public LocId MessageSelf = "generic-injector-component-blocked-user";

    /// <summary>
    /// The message to popup when injection fails on the target
    /// </summary>
    [DataField]
    public LocId MessageOther = "generic-injector-component-blocked-other";
}
