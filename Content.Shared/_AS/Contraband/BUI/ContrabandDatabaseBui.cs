using Robust.Shared.Serialization;

namespace Content.Shared._AS.Contraband.BUI;

[Serializable, NetSerializable]
public enum ContrabandDatabaseUiKey : byte
{
    Key
}

/// <summary>
/// Network state for contraband database UI.
/// Contains per-character statistics for display.
/// </summary>
[Serializable, NetSerializable]
public sealed class ContrabandDatabaseState : BoundUserInterfaceState
{
    public Dictionary<string, CharacterContrabandData> CharacterData { get; set; } = new();
}

/// <summary>
/// Per-character contraband data for network transmission.
/// </summary>
[Serializable, NetSerializable]
public sealed class CharacterContrabandData
{
    public int TotalTurnedIn { get; set; }
    public int TotalRegistered { get; set; }
    public int TotalSold { get; set; }
    public int ScuEarned { get; set; }
    public int EcEarned { get; set; }
    public Dictionary<string, int> TurnedInItems { get; set; } = new();
    public Dictionary<string, int> RegisteredItems { get; set; } = new();
    public Dictionary<string, int> SoldItems { get; set; } = new();
}
