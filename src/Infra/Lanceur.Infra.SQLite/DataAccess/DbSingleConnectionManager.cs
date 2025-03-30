using System.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public static class SQLiteSingleConnectionManagerExtensions
{
    #region Methods

    public static IDbConnection GetConnection(this DbSingleConnectionManager manager) => manager.Connection;

    #endregion
}

/// <summary>
///     Maintain a unique <see cref="IDbConnection" /> throughout the whole
///     lifecycle of the connection manager
/// </summary>
public sealed class DbSingleConnectionManager : IDbConnectionManager
{
    #region Constructors

    public DbSingleConnectionManager(IDbConnection connection) => Connection =
        connection ?? throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL).");

    #endregion

    #region Properties

    internal IDbConnection Connection { get; }

    #endregion

    #region Methods

    public void Dispose() => Connection.Dispose();

    /// <inheritdoc />
    public TReturn WithConnection<TReturn>(Func<IDbConnection, TReturn> action)
    {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        return action(Connection);
    }

    /// <inheritdoc />
    public void WithConnection(Action<IDbConnection> action)
    {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        action(Connection);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public TContext WithinTransaction<TContext>(Func<IDbTransaction, TContext,  TContext> action, TContext context)
    {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        using var tx = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            var result = action(tx, context);
            tx.Commit();
            return result;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    /// <inheritdoc />
    public void WithinTransaction<TContext>(Action<IDbTransaction, TContext> action, TContext context)
    {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        using var tx = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            action(tx, context);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    #endregion
}