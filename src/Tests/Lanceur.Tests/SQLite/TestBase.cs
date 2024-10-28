using Dapper;
using Lanceur.Scripts;
using System.Data;
using System.Data.SQLite;
using System.SQLite.Updater;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Tests.Tooling.SQL;
using Microsoft.Reactive.Testing;
using Xunit.Abstractions;

namespace Lanceur.Tests.SQLite;

public class TestBase
{
    #region Fields

    private const string ConnectionString = "Data Source =:memory: ";

    #endregion

    #region Constructors

    public TestBase(ITestOutputHelper outputHelper) => OutputHelper = outputHelper;

    #endregion

    #region Properties

    protected ITestOutputHelper OutputHelper { get; }

    #endregion

    #region Methods

    protected IDbConnection BuildConnection()
    {
        var conn = new SQLiteConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    protected IDbConnection BuildFreshDb(string sql = null)
    {
        var db = BuildConnection();
        var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbededResourcePattern);
        updater.UpdateFromScratch();

        if (sql.IsNullOrEmpty()) return db;

        db.Execute(sql!);

        return db;
    }

    protected static void CreateVersion(IDbConnection db, string version)
    {
        var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
        db.Execute(sql);
    }

    protected DbSingleConnectionManager GetDatabase(SqlBuilder builder)
    {
        DbSingleConnectionManager connectionManager = null;
        try
        {
            var database = BuildFreshDb(builder.ToString());
            connectionManager = new(database);
            return connectionManager;
        }
        catch
        {
            connectionManager?.Dispose();
            throw;
        }
    }

    #endregion
}