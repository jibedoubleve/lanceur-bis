using System.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public interface IDbConnectionManager : IDisposable
{
    #region Methods

    void WithinTransaction(Action<IDbTransaction> action);

    TReturn WithinTransaction<TReturn>(Func<IDbTransaction, TReturn> action);

    #endregion Methods
}