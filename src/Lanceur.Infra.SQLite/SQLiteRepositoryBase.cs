namespace Lanceur.Infra.SQLite
{
    public abstract class SQLiteRepositoryBase : IDisposable
    {
        #region Constructors

        protected SQLiteRepositoryBase(SQLiteConnectionScope scope)
        {
            DB = scope;
        }

        #endregion Constructors

        #region Properties

        protected SQLiteConnectionScope DB
        {
            get;
        }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            if (DB.Connection is not null)
            {
                DB.Connection.Close();
                DB.Connection.Dispose();
            }
        }

        #endregion Methods
    }
}