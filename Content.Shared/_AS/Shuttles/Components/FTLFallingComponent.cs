using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._AS.Shuttles.Components;

/// <summary>
///     Added to entities which have started falling into FTL Space.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class FTLFallingComponent : Component
{
    /// <summary>
    ///     Time it should take for the falling animation (scaling down) to complete.
    /// </summary>
    [DataField("animationTime")]
    public TimeSpan AnimationTime = TimeSpan.FromSeconds(1.5f);

    /// <summary>
    ///     Time it should take in seconds for the entity to actually fall
    /// </summary>
    [DataField("fallingTime")]
    public TimeSpan FallingTime = TimeSpan.FromSeconds(1.8f);

    [DataField("nextFallingTime", customTypeSerializer:typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextFallingTime = TimeSpan.Zero;

    /// <summary>
    ///     The minimum distance you can reappear from the center of the map
    /// </summary>
    [DataField("minimumDistance")]
    public float MinimumDistance = 2000f;

    /// <summary>
    ///     The minimum distance you can reappear from the center of the map
    /// </summary>
    [DataField("maximumDistance")]
    public float MaximumDistance = 4000;

    /// <summary>
    ///     Original scale of the object so it can be restored
    /// </summary>
    public Vector2 OriginalScale = Vector2.Zero;

    /// <summary>
    ///     Scale that the animation should bring entities to.
    /// </summary>
    public Vector2 AnimationScale = new Vector2(0.01f, 0.01f);
}
