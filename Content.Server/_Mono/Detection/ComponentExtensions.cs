// Monolith Station ship weapons component extensions
// Adds heat signature ratio fields for thermal detection

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Power.Components;

/// <summary>
/// Extension to PowerSupplierComponent for thermal signature tracking
/// </summary>
public sealed partial class PowerSupplierComponent
{
    /// <summary>
    /// Ratio of power supply that generates heat signature (heat units per watt)
    /// </summary>
    [DataField]
    public float HeatSignatureRatio = 0.01f;
}
