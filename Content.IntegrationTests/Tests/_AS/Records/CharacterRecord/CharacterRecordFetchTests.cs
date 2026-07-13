using System.Collections.Generic;
using System.Linq;
using System.Net;
using Content.Server.Database;
using Content.Shared.Preferences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Network;
using Robust.Shared.Serialization.Manager;
using Robust.UnitTesting;


namespace Content.IntegrationTests.Tests._AS.Records.CharacterRecord;

[TestFixture]
public sealed class CharacterRecordFetchTests
{
    private static ServerDbSqlite GetDb(RobustIntegrationTest.ServerIntegrationInstance server)
    {
        var cfg = server.ResolveDependency<IConfigurationManager>();
        var serialization = server.ResolveDependency<ISerializationManager>();
        var opsLog = server.ResolveDependency<ILogManager>().GetSawmill("db.ops");
        var builder = new DbContextOptionsBuilder<SqliteServerDbContext>();
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        builder.UseSqlite(conn);
        return new ServerDbSqlite(() => builder.Options, true, cfg, true, opsLog, serialization);
    }

    [Test]
    public async Task CreateFetchCharacterRecordTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);

        var created = await db.AddCharacterRecord(RecordType.PersonalNote, null, null, null, null);
        var records = await db.GetFilteredCharacterRecords(null);
        Assert.That(records.Any(r => r.Id == created.Id), Is.True, "CharacterRecord did not survive create/retrieve round trip.");

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FilterRecordTypeTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);

        const RecordType controlValue = RecordType.PersonalNote;
        const RecordType testValue = RecordType.SleNote;
        List<RecordCharacter> testRecords =
        [
            new() { RecordType = controlValue },
            new() { RecordType = testValue },
        ];

        await Task.WhenAll(
            testRecords.Select(test =>
                db.AddCharacterRecord(
                    test.RecordType,
                    test.TargetCharacterId,
                    test.AuthorUserId,
                    test.AuthorCharacterId,
                    test.RoundId)));

        var allRecords = await db.GetFilteredCharacterRecords(null);
        var filteredRecords = await db.GetFilteredCharacterRecords(testValue);
        var excludedRecords = allRecords
            .Where(r => filteredRecords.All(f => f.Id != r.Id))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(filteredRecords.All(r => r.RecordType == testValue),
                "Record included in filter did not match filter");

            Assert.That(excludedRecords.Any(r => r.RecordType != testValue),
                "Record excluded from filter matched filter");
        }

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FilterTargetCharacterTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);

        var controlCharacter = await db.InitPrefsAsync(new NetUserId(new Guid("ef67ca59-1129-44ea-9fe2-bb3a094d866d")), new HumanoidCharacterProfile());
        var testCharacter = await db.InitPrefsAsync(new NetUserId(new Guid("e887eb93-f503-4b65-95b6-2f282c014192")), new HumanoidCharacterProfile());
        var controlValue = controlCharacter.Id;
        var testValue = testCharacter.Id;
        List<RecordCharacter> testRecords =
        [
            new(),
            new() { TargetCharacterId = controlValue },
            new() { TargetCharacterId = testValue },
        ];

        await Task.WhenAll(
            testRecords.Select(test =>
                db.AddCharacterRecord(
                    test.RecordType,
                    test.TargetCharacterId,
                    test.AuthorUserId,
                    test.AuthorCharacterId,
                    test.RoundId)));

        var allRecords = await db.GetFilteredCharacterRecords(null);
        var filteredRecords = await db.GetFilteredCharacterRecords(null, targetCharacterId: testValue);
        var excludedRecords = allRecords
            .Where(r => filteredRecords.All(f => f.Id != r.Id))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(filteredRecords.All(r => r.TargetCharacterId == testValue),
                "Record included in filter did not match filter");

            Assert.That(excludedRecords.Any(r => r.TargetCharacterId != testValue),
                "Record excluded from filter matched filter");
        }

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FilterAuthorUserTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);

        var controlValue = new NetUserId(new Guid("e887eb93-f503-4b65-95b6-2f282c014192"));
        var testValue = new NetUserId(new Guid("ef67ca59-1129-44ea-9fe2-bb3a094d866d"));

        await db.UpdatePlayerRecord(controlValue, "Control", new IPAddress(new byte[] { 127, 0, 0, 1 }), null);
        await db.UpdatePlayerRecord(testValue, "Test", new IPAddress(new byte[] { 127, 0, 0, 2 }), null);

        List<RecordCharacter> testRecords =
        [
            new(),
            new() { AuthorUserId = controlValue },
            new() { AuthorUserId = testValue },
        ];

        await Task.WhenAll(
            testRecords.Select(test =>
                db.AddCharacterRecord(
                    test.RecordType,
                    test.TargetCharacterId,
                    test.AuthorUserId,
                    test.AuthorCharacterId,
                    test.RoundId)));

        var allRecords = await db.GetFilteredCharacterRecords(null);
        var filteredRecords = await db.GetFilteredCharacterRecords(null, authorUserId: testValue);
        var excludedRecords = allRecords
            .Where(r => filteredRecords.All(f => f.Id != r.Id))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(filteredRecords.All(r => r.AuthorUserId == testValue),
                "Record included in filter did not match filter");

            Assert.That(excludedRecords.Any(r => r.AuthorUserId != testValue),
                "Record excluded from filter matched filter");
        }

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FilterAuthorCharacterTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);

        var controlCharacter = await db.InitPrefsAsync(new NetUserId(new Guid("ef67ca59-1129-44ea-9fe2-bb3a094d866d")), new HumanoidCharacterProfile());
        var testCharacter = await db.InitPrefsAsync(new NetUserId(new Guid("e887eb93-f503-4b65-95b6-2f282c014192")), new HumanoidCharacterProfile());
        var controlValue = controlCharacter.Id;
        var testValue = testCharacter.Id;
        List<RecordCharacter> testRecords =
        [
            new(),
            new() { AuthorCharacterId = controlValue },
            new() { AuthorCharacterId = testValue },
        ];

        await Task.WhenAll(
            testRecords.Select(test =>
                db.AddCharacterRecord(
                    test.RecordType,
                    test.TargetCharacterId,
                    test.AuthorUserId,
                    test.AuthorCharacterId,
                    test.RoundId)));

        var allRecords = await db.GetFilteredCharacterRecords(null);
        var filteredRecords = await db.GetFilteredCharacterRecords(null, authorCharacterId: testValue);
        var excludedRecords = allRecords
            .Where(r => filteredRecords.All(f => f.Id != r.Id))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(filteredRecords.All(r => r.AuthorCharacterId == testValue),
                "Record included in filter did not match filter");

            Assert.That(excludedRecords.Any(r => r.AuthorCharacterId != testValue),
                "Record excluded from filter matched filter");
        }

        await pair.CleanReturnAsync();
    }
}
