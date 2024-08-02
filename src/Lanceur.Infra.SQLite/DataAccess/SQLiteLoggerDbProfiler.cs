using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public class SQLiteLoggerDbProfiler : LoggerDbProfiler, IDbProfiler
{
    #region Fields

    private readonly ILogger _logger;

    #endregion Fields

    #region Constructors

    public SQLiteLoggerDbProfiler(ILogger logger, bool doFullLogging = false) : base(doFullLogging) => _logger = logger;

    public SQLiteLoggerDbProfiler(ILoggerFactory loggerFactory, bool doFullLogging = false) : base(doFullLogging)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<SQLiteProfiledConnectionFactory>();
    }

    #endregion Constructors

    #region Methods

    public override void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader reader)
    {
        if (DoFullLogging) Log(profiledDbCommand, (sql, parameters) => _logger.LogWarning(Template, "TRACING", sql, parameters));
    }

    public override void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception) => Log(
        profiledDbCommand,
        (sql, parameters) => _logger.LogWarning(Template, "FAILURE", sql, parameters)
    );

    #endregion Methods
}