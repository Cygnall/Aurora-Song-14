// Monolith Station ship weapons component extensions
// Adds heat signature ratio fields for thermal detection

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Extension to ThrusterComponent for thermal signature tracking
/// </summary>
public sealed partial class ThrusterComponent
{
    /// <summary>
    /// Ratio of thrust that generates heat signature (heat units per newton)
    /// </summary>
    [DataField]
    public float HeatSignatureRatio = 100f;
}
