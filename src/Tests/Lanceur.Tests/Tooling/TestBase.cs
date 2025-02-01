using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.SQLite.Updater;
using Dapper;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Tests.Tooling.SQL;
using StackExchange.Profiling.Data;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling;

public class TestBase

{
    #region Fields

    private bool _isProfilingSql;

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

    private DbProfiler SqlProfiler { get;  }

    protected ITestOutputHelper OutputHelper { get; }

    #endregion

    #region Methods
    
    /// <summary>
    /// Activates SQL logging to provide detailed insights into SQL operations executed against the database.
    /// This enables the capturing and display of SQL queries for profiling and debugging purposes.
    /// </summary>
    protected void EnableSqlProfiling() => _isProfilingSql = true;


    protected IDbConnection BuildConnection(string connectionString = null)
    {
        DbConnection connection = new SQLiteConnection(connectionString ?? InMemoryConnectionString);

        if (SqlProfiler is not null) connection = new ProfiledDbConnection(connection, SqlProfiler);

        connection.Open();
        return connection;
    }

    protected IDbConnection BuildFreshDb(string sql = null, string connectionString = null)
    {
        var db = BuildConnection(connectionString);
        var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbeddedResourcePattern);
        updater.UpdateFromScratch();

        if (_isProfilingSql) SqlProfiler.IsActive = true;

        if (sql.IsNullOrEmpty()) return db;

        db.Execute(sql!);

        return db;
    }

    protected static void CreateVersion(IDbConnection db, string version)
    {
        var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
        db.Execute(sql);
    }

    protected DbSingleConnectionManager GetDatabase(SqlBuilder builder, string connectionString = null)
    {
        DbSingleConnectionManager connectionManager = null;
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