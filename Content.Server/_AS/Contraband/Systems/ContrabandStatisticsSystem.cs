using Content.Server._AS.Contraband.Components;
using Content.Server.Station.Systems;
using Content.Shared._AS.Contraband.BUI;
using Content.Shared._AS.Contraband.Components;
using Content.Shared._AS.Contraband.Events;
using Content.Shared.GameTicking;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._AS.Contraband.Systems;

public sealed class ContrabandStatisticsSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
        SubscribeLocalEvent<ContrabandTurnInEvent>(OnContrabandTurnIn);
        SubscribeLocalEvent<ContrabandRegistrationEvent>(OnContrabandRegistration);
        SubscribeLocalEvent<ContrabandSaleEvent>(OnContrabandSale);
        SubscribeLocalEvent<ContrabandDatabaseConsoleComponent, BoundUIOpenedEvent>(OnDatabaseUiOpened);
    }

    private void OnStationInitialized(StationInitializedEvent ev)
    {
        // Add the statistics component to the station entity
        EnsureComp<ContrabandStatisticsComponent>(ev.Station);
    }

    private void OnContrabandTurnIn(ref ContrabandTurnInEvent ev)
    {
        // Find the station for this console
        var station = _stationSystem.GetOwningStation(ev.Console);
        if (station == null)
            return;

        if (!TryComp<ContrabandStatisticsComponent>(station.Value, out var statsComp))
            return;

        // Get or create character stats
        if (!statsComp.CharacterStats.TryGetValue(ev.CharacterName, out var charStats))
        {
            charStats = new CharacterContrabandStats();
            statsComp.CharacterStats[ev.CharacterName] = charStats;
        }

        // Update statistics
        charStats.TotalTurnedIn += ev.ItemPrototypeIds.Count;
        charStats.ScuEarned += ev.ScuValue;
        charStats.EcEarned += ev.EcValue;

        // Track individual items
        foreach (var protoId in ev.ItemPrototypeIds)
        {
            if (!string.IsNullOrEmpty(protoId))
            {
                var entProtoId = new EntProtoId(protoId);
                charStats.TurnedInItems.TryGetValue(entProtoId, out var count);
                charStats.TurnedInItems[entProtoId] = count + 1;
            }
        }

        statsComp.CharacterStats[ev.CharacterName] = charStats;
        Dirty(station.Value, statsComp);

        Log.Info($"Contraband turn-in tracked: {ev.CharacterName} turned in {ev.ItemPrototypeIds.Count} items for {ev.ScuValue} SCU and {ev.EcValue} EC");

        // Update any open database UIs
        UpdateDatabaseTerminalUis(station.Value, statsComp);
    }

    private void OnContrabandRegistration(ref ContrabandRegistrationEvent ev)
    {
        // Find the station for this console
        var station = _stationSystem.GetOwningStation(ev.Console);
        if (station == null)
            return;

        if (!TryComp<ContrabandStatisticsComponent>(station.Value, out var statsComp))
            return;

        // Get or create character stats
        if (!statsComp.CharacterStats.TryGetValue(ev.CharacterName, out var charStats))
        {
            charStats = new CharacterContrabandStats();
            statsComp.CharacterStats[ev.CharacterName] = charStats;
        }

        // Update statistics
        charStats.TotalRegistered += ev.ItemPrototypeIds.Count;

        // Track individual items
        foreach (var protoId in ev.ItemPrototypeIds)
        {
            if (!string.IsNullOrEmpty(protoId))
            {
                var entProtoId = new EntProtoId(protoId);
                charStats.RegisteredItems.TryGetValue(entProtoId, out var count);
                charStats.RegisteredItems[entProtoId] = count + 1;
            }
        }

        if (ev.FirearmSerialNumbers != null)
        {
            foreach (var (serial, protoId) in ev.FirearmSerialNumbers)
            {
                charStats.FirearmSerialNumbers[serial] = new EntProtoId(protoId);
            }
        }

        statsComp.CharacterStats[ev.CharacterName] = charStats;
        Dirty(station.Value, statsComp);

        Log.Info($"Contraband registration tracked: {ev.CharacterName} registered {ev.ItemPrototypeIds.Count} items");

        // Update any open database UIs
        UpdateDatabaseTerminalUis(station.Value, statsComp);
    }

    private void OnContrabandSale(ref ContrabandSaleEvent ev)
    {
        // Find the station for this console
        var station = _stationSystem.GetOwningStation(ev.Console);
        if (station == null)
            return;

        if (!TryComp<ContrabandStatisticsComponent>(station.Value, out var statsComp))
            return;

        // Get or create character stats
        if (!statsComp.CharacterStats.TryGetValue(ev.CharacterName, out var charStats))
        {
            charStats = new CharacterContrabandStats();
            statsComp.CharacterStats[ev.CharacterName] = charStats;
        }

        // Update statistics
        charStats.TotalSold += ev.ItemPrototypeIds.Count;

        // Track individual items
        foreach (var protoId in ev.ItemPrototypeIds)
        {
            if (!string.IsNullOrEmpty(protoId))
            {
                var entProtoId = new EntProtoId(protoId);
                charStats.SoldItems.TryGetValue(entProtoId, out var count);
                charStats.SoldItems[entProtoId] = count + 1;
            }
        }

        statsComp.CharacterStats[ev.CharacterName] = charStats;
        Dirty(station.Value, statsComp);

        Log.Info($"Contraband sale tracked: {ev.CharacterName} sold {ev.ItemPrototypeIds.Count} items for {ev.ScuValue} SCU");

        // Update any open database UIs
        UpdateDatabaseTerminalUis(station.Value, statsComp);
    }

    private void OnDatabaseUiOpened(EntityUid uid, ContrabandDatabaseConsoleComponent component, BoundUIOpenedEvent args)
    {
        var station = _stationSystem.GetOwningStation(uid);
        if (station == null)
            return;

        if (!TryComp<ContrabandStatisticsComponent>(station.Value, out var statsComp))
            return;

        UpdateDatabaseTerminalUi(uid, component, statsComp);
    }

    private void UpdateDatabaseTerminalUis(EntityUid station, ContrabandStatisticsComponent statsComp)
    {
        // Find all database consoles on this station and update their UIs
        var query = EntityQueryEnumerator<ContrabandDatabaseConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (_stationSystem.GetOwningStation(uid) != station)
                continue;

            // Update UI if open
            if (_uiSystem.HasUi(uid, ContrabandDatabaseUiKey.Key))
            {
                UpdateDatabaseTerminalUi(uid, console, statsComp);
            }
        }
    }

    private void UpdateDatabaseTerminalUi(EntityUid uid, ContrabandDatabaseConsoleComponent component, ContrabandStatisticsComponent statsComp)
    {
        var state = new ContrabandDatabaseState();

        foreach (var (characterName, stats) in statsComp.CharacterStats)
        {
            var data = new CharacterContrabandData
            {
                TotalTurnedIn = stats.TotalTurnedIn,
                TotalRegistered = stats.TotalRegistered,
                TotalSold = stats.TotalSold,
                ScuEarned = stats.ScuEarned,
                EcEarned = stats.EcEarned
            };

            // Convert EntProtoId dictionaries to string dictionaries for network transmission
            foreach (var (protoId, count) in stats.TurnedInItems)
            {
                data.TurnedInItems[protoId.Id] = count;
            }

            foreach (var (protoId, count) in stats.RegisteredItems)
            {
                data.RegisteredItems[protoId.Id] = count;
            }

            foreach (var (protoId, count) in stats.SoldItems)
            {
                data.SoldItems[protoId.Id] = count;
            }

            // Copy firearm serial numbers with their prototype IDs
            foreach (var (serial, protoId) in stats.FirearmSerialNumbers)
            {
                data.FirearmSerialNumbers[serial] = protoId.Id;
            }

            state.CharacterData[characterName] = data;
        }

        _uiSystem.SetUiState(uid, ContrabandDatabaseUiKey.Key, state);
    }
}
