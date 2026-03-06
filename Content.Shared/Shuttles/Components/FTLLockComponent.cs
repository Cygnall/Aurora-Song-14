using Robust.Shared.GameStates;
using Robust.Shared.Analyzers;

namespace Content.Shared.Shuttles.Components; // Mono

/// <summary>
/// Component that controls whether a shuttle will FTL with docked shuttles or automatically undock.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FTLLockComponent : Component
{
    /// <summary>
    /// Whether FTL lock is currently enabled
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;
}