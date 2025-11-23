using Robust.Shared.GameStates;

namespace Content.Shared._AS.Weapons.Ranged.Components;

/// <summary>
/// Stores the serial number from the firearm that ejected this casing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CasingSerialNumberComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public string SerialNumber = string.Empty;
}
