// Aurora Song - AS Medical Bounty System
// Based on New Frontier Station 14's Medical Bounty System
// Original implementation: https://github.com/new-frontiers-14/frontier-station-14
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._AS.Medical.Prototypes;

/// <summary>
/// Aurora Song: Prototype for AS medical bounties - defines damage and reagents
/// that must be healed in order to receive a reward.
/// </summary>
[Prototype]
public sealed partial class ASMedicalBountyPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The base monetary reward for a bounty of this type
    /// </summary>
    [DataField(required: true)]
    public int BaseReward;

    /// <summary>
    /// Damage types to be added to a bountied entity and the bonus/penalties associated with them
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdDictionarySerializer<RandomDamagePreset, DamageTypePrototype>))]
    public Dictionary<string, RandomDamagePreset> DamageSets = new();

    /// <summary>
    /// Reagents to be added to a bountied entity and the bonus/penalties associated with them
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdDictionarySerializer<RandomReagentPreset, ReagentPrototype>))]
    public Dictionary<string, RandomReagentPreset> Reagents = new();

    /// <summary>
    /// Penalty for other damage types not in DamageSets on redemption.
    /// </summary>
    [DataField("otherPenalty")]
    public int PenaltyPerOtherPoint = 25;

    /// <summary>
    /// Maximum damage before bounty can be claimed.
    /// </summary>
    [DataField]
    public int MaximumDamageToRedeem = 99;
}

[DataDefinition, Serializable, NetSerializable]
public partial record struct RandomDamagePreset
{
    /// <summary>
    /// The minimum amount of damage to receive.
    /// </summary>
    [DataField("min")]
    public int MinDamage;
    /// <summary>
    /// The maximum amount of damage to receive.
    /// </summary>
    [DataField("max")]
    public int MaxDamage;
    /// <summary>
    /// The value per point of damage.
    /// </summary>
    [DataField("value")]
    public int ValuePerPoint;
    /// <summary>
    /// The penalty per point of damage remaining.
    /// </summary>
    [DataField("penalty")]
    public int PenaltyPerPoint;
}

[DataDefinition, Serializable, NetSerializable]
public partial record struct RandomReagentPreset
{
    /// <summary>
    /// The minimum quantity of reagent to inject.
    /// </summary>
    [DataField("min")]
    public int MinQuantity;
    /// <summary>
    /// The maximum quantity of reagent to inject.
    /// </summary>
    [DataField("max")]
    public int MaxQuantity;
    /// <summary>
    /// The value per unit of reagent.
    /// </summary>
    [DataField("value")]
    public int ValuePerPoint;
}
