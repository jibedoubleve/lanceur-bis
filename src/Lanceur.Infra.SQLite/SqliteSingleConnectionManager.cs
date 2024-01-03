using System.Data;
using System.Data.SQLite;

namespace Lanceur.Infra.SQLite;

public sealed class SQLiteSingleConnectionManager : IDbConnectionManager
{
    #region Fields

    private readonly SQLiteConnection _connection;

    #endregion Fields

    #region Constructors

    public SQLiteSingleConnectionManager(SQLiteConnection connection)
    {
        _connection =
            connection
            ?? throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL).");
    }

    #endregion Constructors

    #region Methods

    public static implicit operator SQLiteConnection(SQLiteSingleConnectionManager manager) => manager._connection;

    public void Dispose() => _connection.Dispose();

    public void WithinTransaction(Action<SQLiteTransaction> action)
    {
        WithinTransaction(tx =>
        {
            action(tx);
            return default(object);
        });
    }

    public TReturn WithinTransaction<TReturn>(Func<SQLiteTransaction, TReturn> action)
    {
        if (_connection.State != ConnectionState.Open) _connection.Open();
        using var tx = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            var result = action(tx);
            tx.Commit();
            return result;
        }
        catch
        {
            tx.Rollback();
            return default;
        }
    }

    #endregion Methods
}