// Aurora Song - AS Medical Bounty System
// Separate implementation from NF Medical Bounty System
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Medical;

/// <summary>
/// Aurora Song: UI key for AS medical bounty redemption interface
/// </summary>
[Serializable, NetSerializable]
public enum ASMedicalBountyRedemptionUiKey : byte
{
    Key
}

/// <summary>
/// Aurora Song: Status enum for AS medical bounty redemption
/// </summary>
[Serializable, NetSerializable]
public enum ASMedicalBountyRedemptionStatus : byte
{
    NoBody,
    NoBounty,
    MedicalOnly,
    TooDamaged,
    NotAlive,
    Valid,
}

/// <summary>
/// Aurora Song: Visual states for AS medical bounty redemption machines
/// </summary>
[Serializable, NetSerializable]
public enum ASMedicalBountyRedemptionVisuals : byte
{
    Full
}

/// <summary>
/// Aurora Song: UI state data for AS medical bounty redemption interface
/// </summary>
[Serializable, NetSerializable]
public sealed class ASMedicalBountyRedemptionUIState : BoundUserInterfaceState
{
    public int BountyValue { get; }
    public ASMedicalBountyRedemptionStatus BountyStatus { get; }
    public bool PaidToStation { get; }

    public ASMedicalBountyRedemptionUIState(ASMedicalBountyRedemptionStatus bountyStatus, int bountyValue, bool paidToStation)
    {
        BountyStatus = bountyStatus;
        BountyValue = bountyValue;
        PaidToStation = paidToStation;
    }
}
