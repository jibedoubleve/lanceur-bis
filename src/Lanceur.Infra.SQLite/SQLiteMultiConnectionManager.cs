using System.Data;
using System.Data.SQLite;

namespace Lanceur.Infra.SQLite
{
    public sealed class SQLiteMultiConnectionManager : IDbConnectionManager
    {
        #region Fields

        private static readonly object Locker = new();
        private readonly string _connectionString;
        private SQLiteConnection _currentConnection;

        #endregion Fields

        #region Constructors

        public SQLiteMultiConnectionManager(SQLiteConnection connection)
        {
            _connectionString =
                connection?.ConnectionString
                ?? throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL).");
        }

        #endregion Constructors

        #region Methods

        private SQLiteConnection GetConnection(bool renewConnection = true)
        {
            lock (Locker)
            {
                if (renewConnection && _currentConnection is not null)
                {
                    _currentConnection.Dispose();
                }

                if (renewConnection)
                {
                    _currentConnection = new(_connectionString);
                }

                return _currentConnection;
            }
        }

        public static implicit operator SQLiteConnection(SQLiteMultiConnectionManager manager) => manager.GetConnection(renewConnection: false);

        public void Dispose()
        {
            // TODO release managed resources here
        }

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
            using var conn = GetConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

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
                return default;
            }
        }

        #endregion Methods
    }
}