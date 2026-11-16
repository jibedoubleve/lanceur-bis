using System.Data;
using System.Data.SQLite;
using System.SQLite.Updater;
using Dapper;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Scripts;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Tests.Tools.SQL;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.Tools;

public abstract class TestBase
{
    #region Fields

    private const string InMemoryConnectionString = "Data Source =:memory:";

    private ILoggerFactory? _loggerFactory;

    #endregion

    #region Constructors

    protected TestBase(ITestOutputHelper outputHelper) => OutputHelper = outputHelper;

    #endregion

    #region Properties

    protected ILoggerFactory LoggerFactory
    {
        get
        {
            if (_loggerFactory != null) { return _loggerFactory; }

            var xunitLoggerOptions = new XUnitLoggerOptions();
            _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder
                .AddProvider(
                    new XUnitLoggerProvider(
                        OutputHelper,
                        xunitLoggerOptions
                    )
                )
                .SetMinimumLevel(
                    LogLevel.Trace
                )
            );

            return _loggerFactory;
        }
    }

    protected ITestOutputHelper OutputHelper { get; }

    #endregion

    #region Methods

    private IDbConnection BuildFreshDb(string? sql = null, string? connectionString = null)
    {
        var db = BuildConnection(connectionString);
        var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbeddedResourcePattern);
        updater.UpdateFromScratch();

        if (!sql.IsNullOrEmpty()) { db.Execute(sql!); }

        return db;
    }

    protected IDbConnection BuildConnection(string? connectionString = null)
    {
        var connection = new SQLiteConnection(connectionString ?? InMemoryConnectionString);

        OutputHelper.WriteLine($"Connection string: '{connection.ConnectionString}'");
        connection.Open();
        return connection;
    }

    protected IDbConnection BuildFreshDb(string? sql = null, IConnectionString? connectionString = null)
        => BuildFreshDb(sql, connectionString?.ToString());

    protected ILogger<T> CreateLogger<T>() => new TestOutputHelperDecoratorForMicrosoftLogging<T>(OutputHelper);
    protected ILoggerFactory CreateLoggerFactory() => new MicrosoftLoggingLoggerFactory(OutputHelper);


    protected static void CreateVersion(IDbConnection db, string version)
    {
        var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
        db.Execute(sql);
    }

    protected DbSingleConnectionManager GetConnectionManager(ISqlBuilder builder, string? connectionString = null)
    {
        DbSingleConnectionManager? connectionManager = null;
        try
        {
            var database = BuildFreshDb(builder.ToSql(), connectionString);
            connectionManager = new DbSingleConnectionManager(database);
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