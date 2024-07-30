using Dapper;
using Lanceur.Scripts;
using System.Data;
using System.Data.SQLite;
using System.SQLite.Updater;
using Microsoft.Reactive.Testing;
using Xunit.Abstractions;

namespace Lanceur.Tests.SQLite
{
    public class TestBase : ReactiveTest
    {
        #region Fields

        private const string ConnectionString = "Data Source =:memory: ";

        #endregion Fields

        #region Constructors

        public TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        #endregion Constructors

        #region Properties

        protected ITestOutputHelper OutputHelper { get; }

        #endregion Properties

        #region Methods

        protected static void CreateVersion(IDbConnection db, string version)
        {
            var sql = $"insert into settings (s_key, s_value) values ('db_version', '{version}')";
            db.Execute(sql);
        }

        protected IDbConnection BuildConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        protected IDbConnection BuildFreshDb(string sql = null)
        {
            var db = BuildConnection();
            var updater = new DatabaseUpdater(db, ScriptRepository.Asm, ScriptRepository.DbScriptEmbededResourcePattern);
            updater.UpdateFromScratch();

            if (sql is not null) { db.Execute(sql); }

            return db;
        }

        #endregion Methods
    }
}