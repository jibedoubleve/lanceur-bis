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
        private readonly SQLiteLoggerDbProfiler _dbProfiler;

        #endregion Fields

        #region Constructors
        public SQLiteProfiledConnectionFactory(string connectionString, ILoggerFactory loggerFactory, bool isFullProvider = false)
        {
            _connectionString = connectionString;
            _dbProfiler = new(loggerFactory, isFullProvider);
        }

        #endregion Constructors

        #region Methods

        public DbConnection CreateConnection() =>
            new ProfiledDbConnection(new SQLiteConnection(_connectionString), _dbProfiler);

        #endregion Methods
    }
}