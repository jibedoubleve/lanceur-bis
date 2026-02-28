using Lanceur.Infra.SQLite.DataAccess;

namespace Lanceur.Infra.SQLite.Repositories;

public abstract class SQLiteRepositoryBase
{
    #region Constructors

    protected SQLiteRepositoryBase(IDbConnectionManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);
        Db = manager;
    }

    #endregion

    #region Properties

    protected IDbConnectionManager Db { get; }

    #endregion
}