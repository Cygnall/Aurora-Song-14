// Aurora Song - AS Medical Bounty System
// Based on New Frontier Station 14's Medical Bounty System
// Original implementation: https://github.com/new-frontiers-14/frontier-station-14
using Content.Shared._AS.Medical.Prototypes;

namespace Content.Shared._AS.Medical;

/// <summary>
/// Aurora Song: Component for entities with AS medical bounties
/// </summary>
[RegisterComponent]
[AutoGenerateComponentState]
public sealed partial class ASMedicalBountyComponent : Component
{
    /// <summary>
    /// Optional: Specific bounty prototype ID to use.
    /// If null, a random bounty will be selected.
    /// </summary>
    [DataField]
    public string? BountyId = null;

    /// <summary>
    /// The bounty to use/used for damage generation.
    /// If null, a medical bounty type will be selected at random.
    /// </summary>
    [DataField(serverOnly: true)]
    public ASMedicalBountyPrototype? Bounty = null;

    /// <summary>
    /// Maximum bounty value for this entity in spesos.
    /// Cached from bounty params on generation.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int MaxBountyValue;

    /// <summary>
    /// Ensures damage is only applied once, set to true on startup.
    /// </summary>
    public bool BountyInitialized;

    /// <summary>
    /// Determines if corpse can only be redeemed by medical staff
    /// </summary>
    [DataField]
    public bool MedicalOnly;
}
