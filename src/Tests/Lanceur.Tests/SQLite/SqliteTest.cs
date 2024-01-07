using System.Data;
using Dapper;
using Lanceur.Scripts;
using System.Data.SQLite;
using System.SQLite.Updater;

namespace Lanceur.Tests.SQLite
{
    public class SQLiteTest
    {
        #region Fields

        private const string ConnectionString = "Data Source =:memory: ";

        #endregion Fields

        #region Methods

        protected static IDbConnection BuildConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        protected static IDbConnection BuildFreshDb(string sql = null)
        {
            var db = BuildConnection();
            var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbededResourcePattern);
            updater.UpdateFromScratch();

            if (sql is not null) { db.Execute(sql); }

            return db;
        }

        protected static void CreateVersion(IDbConnection db, string version)
        {
            var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
            db.Execute(sql);
        }

        #endregion Methods
    }
}