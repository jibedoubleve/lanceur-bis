using Dapper;
using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.SQLite
{
    public class SQLiteVersionManager : SQLiteServiceBase, IDataStoreVersionManager
    {
        #region Constructors

        public SQLiteVersionManager(SQLiteConnectionScope scope) : base(scope)
        {
        }

        #endregion Constructors

        #region Methods

        public Version GetCurrentDbVersion()
        {
            var version = DB.Connection.Query<string>(SQL.GetDbVersion).FirstOrDefault();

            return version.IsNullOrEmpty()
                ? new Version()
                : new Version(version);
        }

        public bool IsUpToDate(Version goalVersion)
        {
            var version = DB.Connection.Query<string>(SQL.GetDbVersion).FirstOrDefault();

            var currentVersion = version.IsNullOrEmpty()
                ? new Version()
                : new Version(version);

            return goalVersion <= currentVersion;
        }

        public bool IsUpToDate(string expectedVersion) => IsUpToDate(new Version(expectedVersion));

        public void SetCurrentDbVersion(Version version)
        {
            var exists = DB.Connection.ExecuteScalar<int>(SQL.DbVersionCount) >= 1;

            if (exists)
            {
                DB.Connection.Execute(SQL.UpdateDbVersion.Format(version));
            }
            else
            {
                DB.Connection.Execute(SQL.SetDbVersion.Format(version));
            }
        }

        #endregion Methods

        #region Classes

        private class SQL
        {
            #region Fields

            public const string DbVersionCount = @"select count(*) from settings where lower(s_key) = 'db_version'";
            public const string GetDbVersion = @"select s_value from settings where lower(s_key) = 'db_version'";
            public const string SetDbVersion = @"insert into settings (s_key, s_value) values ('db_version','{0}');";
            public const string UpdateDbVersion = "update settings set s_value = '{0}' where lower(s_key) = 'db_version'";

            #endregion Fields
        }

        #endregion Classes
    }
}