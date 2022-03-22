using Dapper;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using System.Diagnostics;

namespace Lanceur.Infra.SQLite
{
    public class SQLiteAppSettingsService : SQLiteServiceBase, IAppSettingsService
    {
        #region Fields

        private readonly string[] Keys = new string[]
        {
            "IdSession",
            "ShowAtStartup",
            "HotKey.Key",
            "HotKey.ModifierKeys",
            "Repository.ScoreLimit",
            "Window.Position.Left",
            "Window.Position.Top",
        };

        #endregion Fields

        #region Constructors

        public SQLiteAppSettingsService(SQLiteConnectionScope scope) : base(scope)
        {
        }

        #endregion Constructors

        #region Methods

        public AppSettings Load()
        {
            var sql = @"
            select
	            s_key   as Key,
                s_value as Value
            from settings
            where
	            s_key in (
                  'IdSession',
	              'ShowAtStartup',
                  'HotKey.Key',
	              'HotKey.ModifierKeys',
	              'Repository.ScoreLimit',
	              'Window.Position.Left',
	              'Window.Position.Top'
                );";

            var values = DB.Connection.Query<KeyValue>(sql).ToList();

            if (values != null)
            {
                var settings = new AppSettings();

                int i = 0;
                var key = Keys[i++];
                if (Any(values, key)) { settings.IdSession = values.Where(x => x.Key == key).First().Value.CastToInt(); }

                key = Keys[i++];
                if (Any(values, key)) { settings.ShowAtStartup = values.Where(x => x.Key == key).First().Value.CastToBool(); }

                key = Keys[i++];
                var key2 = Keys[i++];
                if (Any(values, key) && Any(values, key2))
                {
                    var k = values.Where(x => x.Key == key).First().Value.CastToInt();
                    var m = values.Where(x => x.Key == key2).First().Value.CastToInt();
                    settings.HotKey = new HotKeySection(m, k);
                }

                key = Keys[i++];
                if (Any(values, key)) { settings.Repository.ScoreLimit = values.Where(x => x.Key == key).First().Value.CastToInt(); }

                key = Keys[i++];
                if (Any(values, key)) { settings.Window.Position.Left = values.Where(x => x.Key == key).First().Value.CastToDouble(); }

                key = Keys[i++];
                if (Any(values, key)) { settings.Window.Position.Top = values.Where(x => x.Key == key).First().Value.CastToDouble(); }
                return settings;
            }
            else { return new(); }
        }

        private static bool Any(List<KeyValue> values, string key) => values.Any(x => x.Key == key);

        public void Save(AppSettings settings)
        {
            var sql = "update settings set s_value = @value where s_key = @key";
            DB.Connection.Execute(sql, new { key = "IdSession", value = settings.IdSession });
            DB.Connection.Execute(sql, new { key = "HotKey.Key", value = settings.HotKey.Key });
            DB.Connection.Execute(sql, new { key = "HotKey.ModifierKeys", value = settings.HotKey.ModifierKey });
            DB.Connection.Execute(sql, new { key = "Repository.ScoreLimit", value = settings.Repository.ScoreLimit });
            DB.Connection.Execute(sql, new { key = "Window.Position.Left", value = settings.Window.Position.Left });
            DB.Connection.Execute(sql, new { key = "Window.Position.Top", value = settings.Window.Position.Top });
            DB.Connection.Execute(sql, new { key = "ShowAtStartup", value = settings.ShowAtStartup });
        }
        public void Edit(Action<AppSettings> edit)
        {
            var stg = Load();
            edit(stg);
            Save(stg);
        }


        #endregion Methods

        #region Classes

        [DebuggerDisplay("K: {Key} | V: {Value}")]
        public class KeyValue
        {
            #region Properties

            public string Key { get; set; }
            public string Value { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}