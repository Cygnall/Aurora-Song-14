using Content.Server.Database;
using Content.Shared._AS.PersistentSystems;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Serialization.Manager;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests._AS.Records.CharacterRecord;

[TestFixture]
public sealed class CharacterRecordDeleteTests
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
    public async Task DeleteCharacterRecordTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);
        var record = await db.AddCharacterRecord(RecordType.PersonalNote, null, null, null, null);

        await db.DeleteRecord(record.Id, null);
        var query = await db.GetFilteredCharacterRecords(RecordType.PersonalNote, deleted: null);
        Assert.That(query, Is.Not.Empty, "Deleted record not returned with deleted override.");
        Assert.That(query.Find(r => r.Id == record.Id).Deleted, Is.True, "Deleted flag not set.");
        query = await db.GetFilteredCharacterRecords(RecordType.PersonalNote);
        Assert.That(query, Is.Empty, "Deleted record returned.");

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task UndeleteCharacterRecordTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);
        var record = await db.AddCharacterRecord(RecordType.PersonalNote, null, null, null, null);

        await db.DeleteRecord(record.Id, null);
        await db.UndeleteRecord(record.Id, null);
        var query = await db.GetFilteredCharacterRecords(RecordType.PersonalNote);
        Assert.That(query, Is.Not.Empty, "Deleted record not restored.");
        Assert.That(query.Find(r => r.Id == record.Id).Deleted, Is.False, "Deleted flag not unset.");

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task DeleteResultCharacterRecordTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);
        var record = await db.AddCharacterRecord(RecordType.PersonalNote, null, null, null, null);

        Assert.That(await db.DeleteRecord(-1, null),
            Is.EqualTo(RecordUpdateStatus.NotFound),
            "Deleting a record id that does not exist should report NotFound.");

        Assert.That(await db.DeleteRecord(record.Id, null),
            Is.EqualTo(RecordUpdateStatus.Updated),
            "Successfully deleting a non-deleted record should report Updated.");

        Assert.That(await db.DeleteRecord(record.Id, null),
            Is.EqualTo(RecordUpdateStatus.NoChange),
            "Deleting an already deleted record should report NoChange.");

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task UndeleteResultCharacterRecordTest()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);
        var record = await db.AddCharacterRecord(RecordType.PersonalNote, null, null, null, null);

        Assert.That(await db.UndeleteRecord(record.Id, null),
            Is.EqualTo(RecordUpdateStatus.NoChange),
            "Undeleting a non-deleted record should report NoChange.");

        await db.DeleteRecord(record.Id, null);

        Assert.That(await db.UndeleteRecord(-1, null),
            Is.EqualTo(RecordUpdateStatus.NotFound),
            "Undeleting a record id that does not exist should report NotFound.");

        Assert.That(await db.UndeleteRecord(record.Id, null),
            Is.EqualTo(RecordUpdateStatus.Updated),
            "Successfully restoring a deleted record should report Updated.");

        await pair.CleanReturnAsync();
    }
}
