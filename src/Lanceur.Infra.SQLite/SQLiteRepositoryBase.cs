namespace Lanceur.Infra.SQLite
{
    public abstract class SQLiteRepositoryBase 
    {
        #region Constructors

        protected SQLiteRepositoryBase(ISQLiteConnectionScope scope)
        {
            ArgumentNullException.ThrowIfNull(scope);
            DB = scope;
        }

        #endregion Constructors

        #region Properties

        protected ISQLiteConnectionScope DB
        {
            get;
        }

        #endregion Properties
    }
}