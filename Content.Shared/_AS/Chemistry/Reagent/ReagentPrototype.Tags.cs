using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.Reagent;

public sealed partial class ReagentPrototype
{
    [DataField]
    public HashSet<ProtoId<TagPrototype>> Tags = [];
}
