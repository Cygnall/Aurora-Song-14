using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Serialization.Manager;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests._AS.Preferences;

[TestFixture]
public sealed class ServerDbSqliteTests
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

    /// <summary>
    /// Checks that a profile id populates on conversion.
    /// </summary>
    [Test]
    public async Task TestProfileIdPropagation()
    {
        var pair = await PoolManager.GetServerClient();
        var db = GetDb(pair.Server);
        var preferences = (ServerPreferencesManager)pair.Server.ResolveDependency<IServerPreferencesManager>();
        var username = new NetUserId(new Guid("ef67ca59-1129-44ea-9fe2-bb3a094d866d"));
        const int slot = 0;
        await db.InitPrefsAsync(username, new HumanoidCharacterProfile());
        var prefs = await db.GetPlayerPreferencesAsync(username);
        var dbProfile = prefs!.Profiles.Find(p => p.Slot == slot);

        var converted = preferences.ConvertProfiles(dbProfile);
        Assert.That(converted.ProfileId, Is.Not.Null);
        Assert.That(converted.ProfileId, Is.EqualTo(dbProfile.Id));
    }

}
