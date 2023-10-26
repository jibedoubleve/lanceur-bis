namespace Lanceur.Infra.SQLite
{
    public abstract class SQLiteRepositoryBase 
    {
        #region Constructors

        protected SQLiteRepositoryBase(IDbConnectionManager manager)
        {
            ArgumentNullException.ThrowIfNull(manager);
            DB = manager;
        }

        #endregion Constructors

        #region Properties

        protected IDbConnectionManager DB
        {
            get;
        }

        #endregion Properties
    }
}