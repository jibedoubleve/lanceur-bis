using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Services
{
    public class MemoryDatabaseConfigService : IDatabaseConfigRepository
    {
        #region Fields

        private static readonly DatabaseConfig _settings;

        #endregion Fields

        #region Constructors

        static MemoryDatabaseConfigService()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = Path.Combine(desktop, "debug.sqlite");

            _settings = new DatabaseConfig { DbPath = path };
        }

        #endregion Constructors

        #region Properties

        public IDatabaseConfig Current => _settings;

        #endregion Properties

        #region Methods

        public void Load()
        { /*Does nothing, settings is already in memory*/ }

        public void Save()
        { /*Does nothing, settings is already in memory*/ }

        #endregion Methods
    }
}