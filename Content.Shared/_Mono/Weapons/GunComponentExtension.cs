using Content.Shared._Mono.Detection;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Extension to GunComponent for thermal signature support.
/// Adds heat generation per shot for thermal detection systems.
/// </summary>
public sealed partial class GunComponent
{
    /// <summary>
    /// If we have a <see cref="ThermalSignatureComponent"/>, how much heat to generate per shot.
    /// </summary>
    [DataField]
    public float ShootThermalSignature = 0f;
}
