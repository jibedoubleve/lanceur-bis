using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Data;

namespace Lanceur.Infra.SQLite.DataAccess
{

    public class SQLiteLoggerDbProfiler : IDbProfiler
    {
        #region Fields

        private const string Template = "[{Status}] SQL query: {SqlQuery} - {SqlParameters}";
        private readonly ILogger<SQLiteProfiledConnectionFactory> _logger;

        #endregion Fields

        #region Constructors

        public SQLiteLoggerDbProfiler(ILoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory);
            _logger = loggerFactory.CreateLogger<SQLiteProfiledConnectionFactory>();
        }

        #endregion Constructors

        #region Properties

        public bool IsActive => true;

        #endregion Properties

        #region Methods

        private static void Log(IDbCommand profiledDbCommand, Action<string, object[]> log)
        {
            var parameters = profiledDbCommand.Parameters.Cast<SQLiteParameter>()
                                              .Select(x => $"{x.ParameterName}: {x.Value}\r\n")
                                              .ToArray<object>();
            var sql = profiledDbCommand.CommandText;
            log(sql, parameters);
        }

        public void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader reader) { }

        public void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType) { }

        public void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception) => Log(
            profiledDbCommand, (sql, parameters) => _logger.LogWarning(Template, "FAILURE", sql, parameters));

        public void ReaderFinish(IDataReader reader) { }

        #endregion Methods
    }
}