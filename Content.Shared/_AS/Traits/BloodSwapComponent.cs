using Robust.Shared.GameStates;

namespace Content.Shared._AS.Traits;

/// <summary>
/// Component for swapping blood reagents. Used with <see cref="BloodSwapSystem"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(BloodSwapSystem))]
public sealed partial class BloodSwapComponent : Component
{
    /// <summary>
    /// What reagent you're trying to swap the bloodstream to.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public string? BloodReagent;
}
