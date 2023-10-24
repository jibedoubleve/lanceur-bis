using Dapper;
using Lanceur.Scripts;
using Lanceur.Views;
using System.Data.SQLite;
using System.Reflection;
using System.SQLite.Updater;

namespace Lanceur.Tests.SQLite
{
    public class SQLiteTest
    {
        #region Fields

        protected const string ConnectionString = "Data Source =:memory: ";

        #endregion Fields

        #region Methods

        protected static SQLiteConnection BuildConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }


        protected static SQLiteConnection BuildFreshDB(string sql = null)
        {
            var db = BuildConnection();
            var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbededResourcePattern);
            updater.UpdateFromScratch();

            if (sql is not null) { db.Execute(sql); }

            return db;
        }

        protected static void CreateVersion(SQLiteConnection db, string version)
        {
            var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
            db.Execute(sql);
        }

        #endregion Methods
    }
}