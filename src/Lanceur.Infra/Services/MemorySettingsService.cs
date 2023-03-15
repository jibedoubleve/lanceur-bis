using Lanceur.Core.Services;

namespace Lanceur.Infra.Services
{
    public class MemorySettingsService : ISettingsService
    {
        #region Fields

        private static readonly Dictionary<string, string> _settings = null;

        #endregion Fields
        static MemorySettingsService()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = Path.Combine(desktop, "debug.sqlite");

            _settings = new Dictionary<string, string>()
            {
                { "dbpath", path }
            };
        }
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

        public void Load()
        {
        }

        public void Save() { /*Does nothing, settings is already in memory*/ }

        #endregion Methods
    }
}