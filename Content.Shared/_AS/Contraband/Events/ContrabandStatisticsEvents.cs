using Robust.Shared.Serialization;

namespace Content.Shared._AS.Contraband.Events;

/// <summary>
/// Event raised when contraband is turned in at a contraband pad.
/// </summary>
[ByRefEvent]
public record struct ContrabandTurnInEvent(
    EntityUid Actor,
    string CharacterName,
    List<string> ItemPrototypeIds,
    int ScuValue,
    int EcValue,
    EntityUid Console,
    Dictionary<string, string>? FirearmSerialNumbers = null
);

/// <summary>
/// Event raised when contraband is registered (legalized) at a contraband pad.
/// </summary>
[ByRefEvent]
public record struct ContrabandRegistrationEvent(
    EntityUid Actor,
    string CharacterName,
    List<string> ItemPrototypeIds,
    EntityUid Console,
    Dictionary<string, string>? FirearmSerialNumbers = null
);

/// <summary>
/// Event raised when items are sold on a pallet.
/// </summary>
[ByRefEvent]
public record struct ContrabandSaleEvent(
    EntityUid Actor,
    string CharacterName,
    List<string> ItemPrototypeIds,
    int ScuValue,
    EntityUid Console,
    Dictionary<string, string>? FirearmSerialNumbers = null
);
