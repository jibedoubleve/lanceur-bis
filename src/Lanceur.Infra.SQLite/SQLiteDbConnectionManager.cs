using System.Data;
using Lanceur.SharedKernel.Mixins;
using System.Data.SQLite;

namespace Lanceur.Infra.SQLite
{
    public sealed class SQLiteDbConnectionManager : IDbConnectionManager
    {
        #region Fields

        private readonly SQLiteConnection _connection;

        #endregion Fields

        #region Constructors

        public SQLiteDbConnectionManager(SQLiteConnection connection)
        {
            _connection = 
                connection 
                ?? throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL).");
        }

        #endregion Constructors

        #region Methods

        public static implicit operator SQLiteConnection(SQLiteDbConnectionManager manager) => manager._connection;

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public void WithinTransaction(Action<SQLiteTransaction> action)
        {
            if(_connection.State != ConnectionState.Open)_connection.Open();
            using var tx = _connection.BeginTransaction();
            try
            {
                action(tx);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
            }
        }

        public TReturn WithinTransaction<TReturn>(Func<SQLiteTransaction, TReturn> action)
        {     
            if(_connection.State != ConnectionState.Open)_connection.Open();
            using var tx = _connection.BeginTransaction();
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
}