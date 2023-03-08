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
            "HotKey.ModifierKey",
            "Repository.ScoreLimit",
            "Window.Position.Left",
            "Window.Position.Top",
            "RestartDelay",
        };

        #endregion Fields

        #region Constructors

        public SQLiteAppSettingsService(SQLiteConnectionScope scope) : base(scope)
        {
        }

        #endregion Constructors

        #region Methods

        public void Edit(Action<AppSettings> edit)
        {
            var stg = Load();
            edit(stg);
            Save(stg);
        }

        public AppSettings Load()
        {
            var sql = @"
            select
	            s_key   as Key,
                s_value as Value
            from settings
            where
	            s_key in @keys";

            var s = DB.Connection
                .Query(sql, new { keys = Keys })
                .ToDictionary(
                    row => (string)row.Key,
                    row => (object)row.Value
                );
            if (s != null)
            {
                var settings = new AppSettings();
                s.TryGetValue("IdSession", out var val);
                settings.IdSession = val.CastToInt();

                s.TryGetValue("ShowAtStartup", out val);
                settings.ShowAtStartup = val.CastToBool();

                s.TryGetValue("HotKey.Key", out var key);
                s.TryGetValue("HotKey.ModifierKey", out var mod);
                settings.HotKey = new HotKeySection(
                     mod.CastToInt(),
                     key.CastToInt()
                );

                s.TryGetValue("Repository.ScoreLimit", out val);
                settings.Repository.ScoreLimit = val.CastToInt();

                s.TryGetValue("Window.Position.Left", out val);
                settings.Window.Position.Left = val.CastToDouble();

                s.TryGetValue("Window.Position.Top", out val);
                settings.Window.Position.Top = val.CastToDouble();

                s.TryGetValue("RestartDelay", out val);
                settings.RestartDelay = val.CastToInt();

                return settings;
            }
            else { return new(); }
        }

        public void Save(AppSettings settings)
        {
            var sql = "update settings set s_value = @value where s_key = @key";

            foreach (var item in Keys)
            {
                var value = settings.GetPropValue(item);
                DB.Connection.Execute(sql, new { key = item, value });
            }
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