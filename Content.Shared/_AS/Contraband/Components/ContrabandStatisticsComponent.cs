using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._AS.Contraband.Components;

/// <summary>
/// Tracks contraband statistics per character on the station entity.
/// This is temporary per-round data and is not persisted to the database.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ContrabandStatisticsComponent : Component
{
    /// <summary>
    /// Dictionary mapping character names to their contraband statistics.
    /// </summary>
    [DataField]
    public Dictionary<string, CharacterContrabandStats> CharacterStats = new();
}

[Serializable, NetSerializable]
[DataRecord]
public record struct CharacterContrabandStats
{
    /// <summary>
    /// Total number of contraband items turned in by this character.
    /// </summary>
    public int TotalTurnedIn;

    /// <summary>
    /// Total number of contraband items registered (legalized) by this character.
    /// </summary>
    public int TotalRegistered;

    /// <summary>
    /// Total number of items sold on pallets by this character.
    /// </summary>
    public int TotalSold;

    /// <summary>
    /// Total SCU value earned from contraband turn-ins.
    /// </summary>
    public int ScuEarned;

    /// <summary>
    /// Total EC value earned from contraband turn-ins.
    /// </summary>
    public int EcEarned;

    /// <summary>
    /// Dictionary of contraband item prototype IDs turned in and their counts.
    /// </summary>
    public Dictionary<EntProtoId, int> TurnedInItems;

    /// <summary>
    /// Dictionary of contraband item prototype IDs registered and their counts.
    /// </summary>
    public Dictionary<EntProtoId, int> RegisteredItems;

    /// <summary>
    /// Dictionary of item prototype IDs sold on pallets and their counts.
    /// </summary>
    public Dictionary<EntProtoId, int> SoldItems;

    public CharacterContrabandStats()
    {
        TotalTurnedIn = 0;
        TotalRegistered = 0;
        TotalSold = 0;
        ScuEarned = 0;
        EcEarned = 0;
        TurnedInItems = new Dictionary<EntProtoId, int>();
        RegisteredItems = new Dictionary<EntProtoId, int>();
        SoldItems = new Dictionary<EntProtoId, int>();
    }
}
