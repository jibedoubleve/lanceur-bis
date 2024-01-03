using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Services;

namespace Lanceur.Infra.Repositories
{
    public class MemoryDatabaseConfigRepository : IDatabaseConfigRepository
    {
        #region Fields

        private static readonly DatabaseConfig Settings;

        #endregion Fields

        #region Constructors

        static MemoryDatabaseConfigRepository()
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var path = Path.Combine(desktop, "debug.sqlite");

            Settings = new DatabaseConfig { DbPath = path };
        }

        #endregion Constructors

        #region Properties

        public IDatabaseConfig Current => Settings;

        #endregion Properties

        #region Methods

        public void Load()
        { /*Does nothing, settings is already in memory*/ }

        public void Save()
        { /*Does nothing, settings is already in memory*/ }

        #endregion Methods
    }
}