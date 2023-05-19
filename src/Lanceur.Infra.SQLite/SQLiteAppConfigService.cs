using Dapper;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services.Config;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite
{
    public class SQLiteAppConfigService : SQLiteServiceBase, IAppConfigService
    {
        #region Fields

        private AppConfig _current;

        #endregion Fields

        #region Constructors

        public SQLiteAppConfigService(SQLiteConnectionScope scope) : base(scope)
        {
        }

        #endregion Constructors

        #region Properties

        public AppConfig Current
        {
            get
            {
                if (_current is null) { Load(); }
                return _current;
            }
        }

        #endregion Properties

        #region Methods

        public void Edit(Action<AppConfig> edit)
        {
            var stg = Current;
            edit(stg);
            Save();
        }

        public void Load()
        {
            var sql = @"
                select
                    s_key   as Key,
                    s_value as Value
                from settings
                where s_key = 'json';";
            var s = DB.Connection
                      .Query<AppConfig>(sql)
                      .FirstOrDefault();
            _current = s ?? new AppConfig();
        }

        public void Save()
        {
            var sqlExists = "select count(*) from settings where s_key = 'json'";
            var exists = DB.Connection.ExecuteScalar<long>(sqlExists) > 0;

            if (!exists)
            {
                var sqlInsert = "insert into settings(s_key) values ('json')";
                DB.Connection.Execute(sqlInsert);
            }

            var sql = "update settings set s_value = @value where s_key = 'json'";
            var json = JsonConvert.SerializeObject(Current);
            DB.Connection.Execute(sql, new { value = json });
        }

        #endregion Methods
    }
}