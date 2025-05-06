using System.Data;
using System.Data.SQLite;
using System.SQLite.Updater;
using Dapper;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Tests.Tools.SQL;
using StackExchange.Profiling.Data;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tools;

public abstract class TestBase
{
    #region Fields

    private const string InMemoryConnectionString = "Data Source =:memory:";

    #endregion

    #region Constructors

    public TestBase(ITestOutputHelper outputHelper)
    {
        SqlProfiler = new(outputHelper);
        OutputHelper = outputHelper;
    }

    #endregion

    #region Properties

    private static bool IsProfilingSql => false;

    private DbProfiler SqlProfiler { get;  }

    protected ITestOutputHelper OutputHelper { get; }

    #endregion

    #region Methods

    protected IDbConnection BuildConnection(string? connectionString = null)
    {
        if (IsProfilingSql) SqlProfiler.IsActive = true;
        var connection = new ProfiledDbConnection(
            new SQLiteConnection(connectionString ?? InMemoryConnectionString),
            SqlProfiler
        );

        OutputHelper.WriteLine($"Connection string: '{connection.ConnectionString}'");
        connection.Open();
        return connection;
    }

    protected IDbConnection BuildFreshDb(string? sql = null, IConnectionString? connectionString = null)
        => BuildFreshDb(sql, connectionString?.ToString());

    private IDbConnection BuildFreshDb(string? sql = null, string? connectionString = null)
    {
        var db = BuildConnection(connectionString);
        var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbeddedResourcePattern);
        updater.UpdateFromScratch();

        if (!sql.IsNullOrEmpty()) db.Execute(sql!);

        return db;
    }


    protected static void CreateVersion(IDbConnection db, string version)
    {
        var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
        db.Execute(sql);
    }

    protected DbSingleConnectionManager GetConnectionManager(SqlBuilder builder, string? connectionString = null)
    {
        DbSingleConnectionManager? connectionManager = null;
        try
        {
            var database = BuildFreshDb(builder.ToString(), connectionString);
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