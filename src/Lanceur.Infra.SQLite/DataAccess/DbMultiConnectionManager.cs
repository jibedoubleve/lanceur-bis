using System.Data;
using System.Data.Common;
using Splat;

namespace Lanceur.Infra.SQLite.DataAccess;

public sealed class DbMultiConnectionManager : IDbConnectionManager
{
    #region Fields

    private static readonly object Locker = new();
    private readonly IDbConnectionFactory _connectionFactory;
    private DbConnection _currentConnection;

    #endregion Fields

    #region Constructors

    public DbMultiConnectionManager(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory ?? Locator.Current.GetService<IDbConnectionFactory>();

    #endregion Constructors

    #region Methods

    private DbConnection GetConnection(bool renewConnection = true)
    {
        lock (Locker)
        {
            if (renewConnection && _currentConnection is not null) _currentConnection.Dispose();

            if (renewConnection) _currentConnection = _connectionFactory.CreateConnection();

            return _currentConnection;
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void WithinTransaction(Action<IDbTransaction> action)
    {
        WithinTransaction(
            tx =>
            {
                action(tx);
                return default(object);
            }
        );
    }

    public TReturn WithinTransaction<TReturn>(Func<IDbTransaction, TReturn> action)
    {
        using var conn = GetConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            var result = action(tx);
            tx.Commit();
            return result;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    #endregion Methods
}