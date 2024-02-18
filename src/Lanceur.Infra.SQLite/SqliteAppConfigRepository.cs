using Dapper;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;

namespace Lanceur.Infra.SQLite
{
    public class SQLiteAppConfigRepository : SQLiteRepositoryBase, IAppConfigRepository
    {
        #region Fields

        private AppConfig _current;

        #endregion Fields

        #region Constructors

        public SQLiteAppConfigRepository(IDbConnectionManager manager) : base(manager)
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
            const string sql = @"
                select
                    s_value as Value
                from settings
                where s_key = 'json';";
            var s = DB.WithinTransaction(tx =>
                tx.Connection.Query<string>(sql)
                    .FirstOrDefault());

            _current = s.IsNullOrEmpty()
                ? new AppConfig()
                : JsonConvert.DeserializeObject<AppConfig>(s);
        }

        public void Save()
        {
            DB.WithinTransaction(tx =>
            {
                var sqlExists = "select count(*) from settings where s_key = 'json'";
                var exists = tx.Connection.ExecuteScalar<long>(sqlExists) > 0;

                if (!exists)
                {
                    var sqlInsert = "insert into settings(s_key) values ('json')";
                    tx.Connection.Execute(sqlInsert);
                }

                var sql = "update settings set s_value = @value where s_key = 'json'";
                var json = JsonConvert.SerializeObject(Current);
                tx.Connection.Execute(sql, new { value = json });
            });
        }

        #endregion Methods
    }
}