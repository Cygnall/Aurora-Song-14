using Content.Shared.Radio.Components;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._EE.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<ActiveRadioComponent, EncryptionChannelsChangedEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, IntrinsicRadioTransmitterComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(uid, args.Component, ref component.Channels, ref component.ReadOnlyChannels); // Aurora's Song
    }

    private void OnReceiverChannelsChanged(EntityUid uid, ActiveRadioComponent component, EncryptionChannelsChangedEvent args)
    {
        HashSet<ProtoId<RadioChannelPrototype>> doNothing = []; // Aurora's Song - I HAVE to do this
        UpdateChannels(uid, args.Component, ref component.Channels, ref doNothing); // Aurora's Song
    }

    private void UpdateChannels(EntityUid _, EncryptionKeyHolderComponent keyHolderComp, ref HashSet<ProtoId<RadioChannelPrototype>> channels, ref HashSet<ProtoId<RadioChannelPrototype>> readonlyChannels) // Aurora's Song
    {
        channels.Clear();
        channels.UnionWith(keyHolderComp.Channels);
        // Aurora's Song Start
        readonlyChannels.Clear();
        readonlyChannels.UnionWith(keyHolderComp.ReadOnlyChannels);
        // Aurora's Song End
    }
}
