using System.Data.Common;
using System.Data.SQLite;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Data;

namespace Lanceur.Infra.SQLite.DataAccess
{
    public class SQLiteProfiledConnectionFactory : IDbConnectionFactory
    {
        #region Fields

        private readonly string _connectionString;
        private readonly ILoggerFactory _loggerFactory;

        #endregion Fields

        #region Constructors

        public SQLiteProfiledConnectionFactory(string connectionString, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _loggerFactory = loggerFactory;
        }

        #endregion Constructors

        #region Methods

        public DbConnection CreateConnection() =>
            new ProfiledDbConnection(new SQLiteConnection(_connectionString), new SQLiteLoggerDbProfiler(_loggerFactory));

        #endregion Methods
    }
}