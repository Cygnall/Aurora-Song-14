using Content.Shared.Metabolism;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Metabolism;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class MetabolizerTagWhitelistComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<TagPrototype>> Tags;

    [DataField(required: true)]
    public List<ProtoId<MetabolismStagePrototype>> Stages;
}
