using System.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public static class SQLiteSingleConnectionManagerMixin
{
    public static IDbConnection GetConnection(this DbSingleConnectionManager manager) => manager.Connection;
}

/// <summary>
/// Maintain a unique <see cref="IDbConnection"/> throughout the whole
/// lifecycle of the connection manager
/// </summary>
public sealed class DbSingleConnectionManager : IDbConnectionManager
{
    #region Fields

    internal IDbConnection Connection { get; }

    #endregion Fields

    #region Constructors

    public DbSingleConnectionManager(IDbConnection connection) => Connection =
        connection ?? throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL).");

    #endregion Constructors

    #region Methods

    public void Dispose() => Connection.Dispose();

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
        if (Connection.State != ConnectionState.Open) Connection.Open();
        using var tx = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
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