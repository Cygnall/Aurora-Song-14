using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;


namespace Content.Shared._AS.PlasmaGun;

/// <summary>
/// This is a rejigged implementation of the PneumaticCannonComponent, tailored to work for a series of plasma guns
/// </summary>

[RegisterComponent, NetworkedComponent]

public sealed partial class PlasmaGunComponent : Component
{
    public const string TankSlotId = "gas_tank";

    /// <summary>
    ///     Amount of moles to consume for each shot at any power.
    /// </summary>
    [DataField("GasUsage")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float GasUsage = 0.142f;

    /// <summary>
    ///     Base projectile speed at default power.
    /// </summary>
    [DataField("baseProjectileSpeed")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float BaseProjectileSpeed = 20f;

    /// <summary>
    ///     The current projectile speed setting.
    /// </summary>
    [DataField]
    public float? ProjectileSpeed;

}
