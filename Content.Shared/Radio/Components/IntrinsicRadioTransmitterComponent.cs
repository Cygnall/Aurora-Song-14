using Content.Shared.Chat;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.Components;

/// <summary>
///     This component allows an entity to directly translate spoken text into radio messages (effectively an intrinsic
///     radio headset).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IntrinsicRadioTransmitterComponent : Component
{
    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> Channels = new() { SharedChatSystem.CommonChannel };

    // Aurora's Song
    /// <summary>
    ///     A list of radio channels that are ReadOnly, you must still include the channel in the channels list.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> ReadOnlyChannels = new();
}
