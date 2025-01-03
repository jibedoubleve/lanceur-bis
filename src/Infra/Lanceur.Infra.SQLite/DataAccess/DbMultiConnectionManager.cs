using System.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public sealed class DbMultiConnectionManager : IDbConnectionManager
{
    #region Fields

    private readonly IDbConnectionFactory _connectionFactory;

    #endregion

    #region Constructors

    public DbMultiConnectionManager(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    #endregion

    #region Methods

    public void Dispose()
    {
        // TODO release managed resources here
    }

    /// <inheritdoc />
    public TReturn WithConnection<TReturn>(Func<IDbConnection, TReturn> action)
    {
        using var conn = _connectionFactory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        return action(conn);
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
        using var conn = _connectionFactory.CreateConnection();
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

    /// <inheritdoc />
    public TContext WithinTransaction<TContext>(Func<IDbTransaction, TContext, TContext> action, TContext context)
    {
        using var conn = _connectionFactory.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();

        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
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

    #endregion
}