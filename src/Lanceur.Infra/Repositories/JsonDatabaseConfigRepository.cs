using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Newtonsoft.Json;

namespace Lanceur.Infra.Repositories
{
    public class JsonDatabaseConfigRepository : IDatabaseConfigRepository
    {
        #region Fields

        private static readonly object _locker = new();
        private readonly string _filePath;
        private IDatabaseConfig _current;

        #endregion Fields

        #region Constructors

        public JsonDatabaseConfigRepository(string path = null)
        {
            path ??= @"%appdata%\probel\lanceur2\settings.json";
            _filePath = Environment.ExpandEnvironmentVariables(path);
        }

        #endregion Constructors

        #region Properties

        public IDatabaseConfig Current
        {
            get
            {
                if (_current is null) { Load(); }
                return _current;
            }
        }

        #endregion Properties

        #region Methods

        private FileStream OpenFile()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(_filePath)) { File.Delete(_filePath); }

            return File.Create(_filePath);
        }

        public void Load()
        {
            lock (_locker)
            {
                DatabaseConfig jsonSettings = null;
                if (File.Exists(_filePath))
                {
                    var output = File.ReadAllText(_filePath);
                    jsonSettings = JsonConvert.DeserializeObject<DatabaseConfig>(output);
                }

                _current = jsonSettings ?? new DatabaseConfig();
            }
        }

        public void Save()
        {
            lock (_locker)
            {
                // If _current is null, it was never loaded, then
                // not modified...
                if (_current is null) { return; }

                var json = JsonConvert.SerializeObject(_current);

                using var stream = OpenFile();
                using var file = new StreamWriter(stream);
                file.Write(json);
            }
        }

        #endregion Methods
    }
}