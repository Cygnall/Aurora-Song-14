using Robust.Shared.GameStates;

namespace Content.Shared._AS.Contraband.Components;

/// <summary>
/// A terminal that can view contraband handling statistics.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ContrabandDatabaseTerminalComponent : Component
{
    /// <summary>
    /// Maximum range to search for a statistics component (typically on the station).
    /// </summary>
    [DataField]
    public float SearchRange = 1000f;
}
