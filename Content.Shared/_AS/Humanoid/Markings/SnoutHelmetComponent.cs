// Originally from https://github.com/DeltaV-Station/Delta-v/pull/3875, Edited by snezshiba with permission

using Content.Shared.Body;
using Content.Shared.Humanoid;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.Humanoid.Markings;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SnoutHelmetComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? AlternateHelmet;

    [DataField]
    public HumanoidVisualLayers Layer = HumanoidVisualLayers.Head;

    [DataField]
    public ProtoId<OrganCategoryPrototype> Organ = "Head";
}
