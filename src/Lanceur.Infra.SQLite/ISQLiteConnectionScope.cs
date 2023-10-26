using System.Data.SQLite;

namespace Lanceur.Infra.SQLite;

public interface ISQLiteConnectionScope : IDisposable
{
    #region Methods

    void WithinTransaction(Action<SQLiteTransaction> action);

    TReturn WithinTransaction<TReturn>(Func<SQLiteTransaction, TReturn> action);

    #endregion Methods
}