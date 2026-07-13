using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Preferences;

[TestFixture]
public sealed class StationSpawnIdCardTests
{
    // Aurora's Song
    /// <summary>
    /// Tests that profile id gets populated on id card when spawned.
    /// </summary>
    [Test]
    public async Task TestProfileIdAppliedToIdCard()
    {
        var pair = await PoolManager.GetServerClient(new PoolSettings()
        {
            Dirty = true,
        });
        var server = pair.Server;

        var entManager = server.ResolveDependency<IEntityManager>();

        // Check that an empty role loadout spawns gear
        var stationSystem = entManager.System<StationSpawningSystem>();
        var inventorySystem = entManager.System<InventorySystem>();
        var testMap = await pair.CreateTestMap();

        const int expectedProfileId = 8008135;

        await server.WaitAssertion(() =>
        {
            var profile = new HumanoidCharacterProfile()
            {
                Name = "John Naked",
                Species = "Human",
                ProfileId = expectedProfileId,
            };

            var mob = stationSystem.SpawnPlayerMob(testMap.GridCoords, job: "Passenger", profile, station: null);

            Assert.That(inventorySystem.TryGetSlotEntity(mob, "id", out var idSlotContents), Is.True,
                "Spawned mob has nothing in its id slot");
            var idCard = idSlotContents!.Value;

            if (entManager.TryGetComponent<PdaComponent>(idCard, out var pda) && pda.ContainedId != null)
                idCard = pda.ContainedId.Value;

            Assert.That(entManager.TryGetComponent<IdCardComponent>(idCard, out var card), Is.True,
                "Resolved id-slot entity has no IdCardComponent.");

            Assert.That(card!.ProfileId, Is.EqualTo(expectedProfileId),
                "ProfileId was not propagated to the ID card on spawn.");
        });

        await pair.CleanReturnAsync();
    }
}
