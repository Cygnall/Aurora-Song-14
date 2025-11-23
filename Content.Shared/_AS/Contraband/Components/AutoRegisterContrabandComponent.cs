namespace Content.Shared._AS.Contraband.Components;

/// <summary>
/// When attached to a vending machine or uplink, automatically registers
/// Class 2 contraband items to the purchaser when purchased.
/// </summary>
[RegisterComponent]
public sealed partial class AutoRegisterContrabandComponent : Component
{
    [DataField, ViewVariables]
    public EntityUid? LastBuyer;

    [DataField, ViewVariables]
    public TimeSpan LastBuyTime;
}
