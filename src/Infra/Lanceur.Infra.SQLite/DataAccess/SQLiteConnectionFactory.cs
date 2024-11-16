using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace Lanceur.Infra.SQLite.DataAccess;

public class SQLiteConnectionFactory : IDbConnectionFactory
{
    #region Fields

    private readonly string _connectionString;

    #endregion Fields

    #region Constructors

    public SQLiteConnectionFactory(string connectionString) => _connectionString = connectionString;

    #endregion Constructors

    #region Methods

    public DbConnection CreateConnection() => new SQLiteConnection(_connectionString);

    #endregion Methods
}