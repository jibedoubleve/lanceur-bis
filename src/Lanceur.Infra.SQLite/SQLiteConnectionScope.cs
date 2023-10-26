using System.Data.SQLite;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.SQLite;

public sealed class SQLiteConnectionScope : ISQLiteConnectionScope
{
    #region Fields

    private readonly string _connectionString;

    #endregion Fields

    #region Constructors

    public SQLiteConnectionScope(SQLiteConnection connection)
    {
        _connectionString = connection?.ConnectionString
                            ?? throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL).");
    }

    public SQLiteConnectionScope(string connectionString)
    {
        if (connectionString.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(connectionString), "The connection string is in an invalid state (it's either null or white space).");
        }
        _connectionString = connectionString;
    }

    #endregion Constructors

    #region Properties

    public string DbPath => _connectionString?.ToLower()?.Replace("data source=", "")?.Replace(";version=3;", "") ?? "";

    #endregion Properties

    #region Methods

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void WithinTransaction(Action<SQLiteTransaction> action) => WithinTransaction<object>(tx =>
    {
        action(tx);
        return default;
    });

    public TReturn WithinTransaction<TReturn>(Func<SQLiteTransaction, TReturn> action)
    {
        using var conn = new SQLiteConnection(_connectionString);
        using var tx = conn.BeginTransaction();
        try
        {
            var result =action(tx);
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