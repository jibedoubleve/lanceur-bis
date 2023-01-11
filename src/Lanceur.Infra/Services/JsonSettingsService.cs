using Lanceur.Core.Services;
using Newtonsoft.Json;

namespace Lanceur.Infra.Services
{
    internal class JsonSettings
    {
        #region Properties

        public string DbPath { get; set; } = @"%appdata%\probel\lanceur2\data.sqlite";

        #endregion Properties
    }

    public class JsonSettingsService : ISettingsService
    {
        #region Fields

        private readonly string _filePath;
        private Dictionary<string, string> _settings = null;

        #endregion Fields

        #region Constructors

        public JsonSettingsService(string path = null)
        {
            path ??= @"%appdata%\probel\lanceur2\settings.json";
            _filePath = Environment.ExpandEnvironmentVariables(path);
        }

        #endregion Constructors

        #region Indexers

        public string this[Setting key]
        {
            get
            {
                if (_settings == null) { Load(); }
                if (!_settings.ContainsKey(key.ToString().ToLower()))
                {
                    throw new NotSupportedException($"Setting key '{key}' does not exist in the settings.");
                }
                return _settings[key.ToString().ToLower()];
            }
            set
            {
                if (_settings == null) { Load(); }
                else if (!_settings.ContainsKey(key.ToString().ToLower()))
                {
                    throw new NotSupportedException($"Setting key '{key}' does not exist in the settings.");
                }

                _settings[key.ToString().ToLower()] = value;
            }
        }

        #endregion Indexers

        #region Methods

        private FileStream OpenFile()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var stream = !File.Exists(_filePath)
                ? File.Create(_filePath)
                : File.Open(_filePath, FileMode.Open);
            return stream;
        }

        public void Load()
        {
            var stg = new JsonSettings();
            if (File.Exists(_filePath))
            {
                var output = File.ReadAllText(_filePath);
                stg = JsonConvert.DeserializeObject<JsonSettings>(output);
            }

            _settings = new()
            {
                { nameof(stg.DbPath).ToLower(), stg?.DbPath ?? string.Empty }
            };

            Save();
        }

        public void Save()
        {
            var settings = new JsonSettings
            {
                DbPath = _settings[Setting.DbPath.ToString().ToLower()]
            };
            var json = JsonConvert.SerializeObject(settings);

            using (var stream = OpenFile())
            using (var file = new StreamWriter(stream))
            {
                file.Write(json);
            }
        }

        #endregion Methods
    }
}