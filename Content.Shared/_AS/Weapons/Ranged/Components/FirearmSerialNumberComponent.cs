using Robust.Shared.GameStates;

namespace Content.Shared._AS.Weapons.Ranged.Components;

/// <summary>
/// Generates a unique 16-character hexadecimal serial number for firearms.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FirearmSerialNumberComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public string SerialNumber = string.Empty;

    /// <summary>
    /// Whether the serial number has been filed off with a welder.
    /// The serial is still stored but hidden from inspection.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool SerialFiled = false;
}
