using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Power.Components;

/// <summary>
/// Adds or removes components when an entity is powered and online.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(PoweredComponentSystem))]
public sealed partial class PoweredComponentComponent : Component
{
    /// <summary>
    /// A master control for whether or not the entity is broken and can function.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Broken;

        /// <summary>
    /// The components to add when activated.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// The components to remove when deactivated.
    /// If this is null <see cref="Components"/> is reused.
    /// </summary>
    [DataField]
    public ComponentRegistry? RemoveComponents;

    /// <summary>
    /// If true, adds components on the entity's parent instead of the entity itself.
    /// </summary>
    [DataField]
    public bool Parent;

    // <summary>
    // It holds the entity that the component gave the component to, so it can remove from it even if it changes parent.
    // </summary>
    [DataField]
    public EntityUid? Target;
}
