using System.Data.Common;
using System.Data.SQLite;
using Lanceur.Core.Utils;
using Microsoft.Extensions.Logging;
using StackExchange.Profiling.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public class SQLiteProfiledConnectionFactory : IDbConnectionFactory
{
    #region Fields

    private readonly string _connectionString;
    private readonly SQLiteLoggerDbProfiler _dbProfiler  ;

    #endregion

    #region Constructors

    public SQLiteProfiledConnectionFactory(IConnectionString connectionString, ILogger<SQLiteProfiledConnectionFactory> loggerFactory, bool isFullProvider = false)
    {
        _connectionString = connectionString.ToString();
        _dbProfiler = new(loggerFactory, isFullProvider);
    }

    #endregion

    #region Methods

    public DbConnection CreateConnection() => new ProfiledDbConnection(new SQLiteConnection(_connectionString), _dbProfiler);

    #endregion
}