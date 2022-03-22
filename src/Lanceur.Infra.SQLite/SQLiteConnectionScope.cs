using Lanceur.SharedKernel.Mixins;
using System.Data.SQLite;

namespace Lanceur.Infra.SQLite
{
    public sealed class SQLiteConnectionScope : IDisposable
    {
        #region Constructors

        public SQLiteConnectionScope(SQLiteConnection connection)
        {
            if (connection is null) { throw new ArgumentNullException(nameof(connection), "Cannot create a connection scope with an empty connection (NULL)."); }
            Connection = connection;
        }

        public SQLiteConnectionScope(string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(connectionString), "The connection string is in an invalid state (it's either null or white space).");
            }
            Connection = new SQLiteConnection(connectionString);
        }

        #endregion Constructors

        #region Properties

        public SQLiteConnection Connection { get; }

        public string DbPath => Connection?.ConnectionString?.ToLower()?.Replace("data source=", "")?.Replace(";version=3;", "") ?? "";

        #endregion Properties

        #region Methods

        public static implicit operator SQLiteConnection(SQLiteConnectionScope scope) => scope.Connection;

        public void Dispose() => Connection.Dispose();

        #endregion Methods
    }
}