using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._AS.WeaponSwap;

/// <summary>
/// Allows swapping weapons for their variants based on a mapping.
/// Example: Standard pistol -> Suppressed pistol
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeaponSwapComponent : Component
{
    /// <summary>
    /// Sound to play when swapping a weapon.
    /// </summary>
    [DataField]
    public SoundSpecifier? SwapSound = new SoundPathSpecifier("/Audio/Items/rped.ogg");

    /// <summary>
    /// How long the swap takes.
    /// </summary>
    [DataField]
    public TimeSpan SwapDuration = TimeSpan.FromSeconds(2.0);

    /// <summary>
    /// Whether to check distance when interacting.
    /// </summary>
    [DataField]
    public bool DoDistanceCheck = true;

    /// <summary>
    /// Current audio stream for the swap sound.
    /// </summary>
    [DataField]
    public EntityUid? AudioStream;

    /// <summary>
    /// If true, require a second confirmation interaction before starting the swap.
    /// This uses simple popups (no custom UI) and a short timeout for the confirmation window.
    /// </summary>
    [DataField]
    public bool RequireConfirmation = false;

    /// <summary>
    /// How long the confirmation is valid for after the first click.
    /// A second click on the same target within this timeframe will begin the swap.
    /// </summary>
    [DataField]
    public TimeSpan ConfirmationTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Tracks the last weapon targeted for confirmation.
    /// </summary>
    [DataField]
    public EntityUid? PendingTarget;

    /// <summary>
    /// When the confirmation for the pending target expires (UTC game time).
    /// </summary>
    [DataField]
    public TimeSpan? PendingExpiry;

    /// <summary>
    /// Mapping of weapon prototype -> variant prototype.
    /// Example: "WeaponPistolMk58" -> "WeaponPistolMk58Suppressed"
    /// </summary>
    [DataField(required: true)]
    public Dictionary<EntProtoId, EntProtoId> WeaponMappings = new();
}
