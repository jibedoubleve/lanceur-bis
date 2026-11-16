using System.Data;
using System.Data.SQLite;
using Lanceur.Core.Utils;

namespace Lanceur.Infra.SQLite.DataAccess;

public sealed class DbMultiConnectionManager : IDbConnectionManager
{
    #region Fields

    private readonly IConnectionString _connectionString;

    #endregion

    #region Constructors

    public DbMultiConnectionManager(IConnectionString connectionString) => _connectionString = connectionString;

    #endregion

    #region Methods

    private IDbConnection CreateConnection() => new SQLiteConnection(_connectionString.ToString());

    public void Dispose()
    {
        /* Release managed resources here */
    }

    /// <inheritdoc />
    public TReturn WithConnection<TReturn>(Func<IDbConnection, TReturn> action)
    {
        using var conn = CreateConnection();
        if (conn.State != ConnectionState.Open) { conn.Open(); }

        return action(conn);
    }

    /// <inheritdoc />
    public void WithConnection(Action<IDbConnection> action)
    {
        using var conn = CreateConnection();
        if (conn.State != ConnectionState.Open) { conn.Open(); }

        action(conn);
    }

    /// <inheritdoc />
    public void WithinTransaction(Action<IDbTransaction> action) =>
        WithinTransaction(tx => {
                action(tx);
                return default(object);
            }
        );

    /// <inheritdoc />
    public TReturn WithinTransaction<TReturn>(Func<IDbTransaction, TReturn> action)
    {
        using var conn = CreateConnection();
        if (conn.State != ConnectionState.Open) { conn.Open(); }

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
        using var conn = CreateConnection();
        if (conn.State != ConnectionState.Open) { conn.Open(); }

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

    /// <inheritdoc />
    public void WithinTransaction<TContext>(Action<IDbTransaction, TContext> action, TContext context)
    {
        using var conn = CreateConnection();
        if (conn.State != ConnectionState.Open) { conn.Open(); }

        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
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