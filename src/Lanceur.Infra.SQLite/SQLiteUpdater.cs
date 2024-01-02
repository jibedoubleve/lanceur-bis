using Lanceur.Core.Managers;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite
{
    public class SQLiteUpdater
    {
        #region Fields

        private readonly ILogger<SQLiteUpdater> _logger;
        private readonly IDataStoreUpdateManager _updater;
        private readonly IDataStoreVersionManager _versionManager;

        #endregion Fields

        #region Constructors

        public SQLiteUpdater(
            IDataStoreVersionManager versionManager,
            ILoggerFactory logService,
            IDataStoreUpdateManager updater
            )
        {
            _versionManager = versionManager;
            _logger = logService.GetLogger<SQLiteUpdater>();
            _updater = updater;
        }

        #endregion Constructors

        #region Methods

        private static void CreateDirectory(string dbPath)
        {
            var dir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
        }

        public void Update(string cString)
        {
            var dbPath = cString.ExtractPathFromSQLiteCString();
            if (!File.Exists(dbPath))
            {
                CreateDirectory(dbPath);
                _logger.LogWarning("Creating a new database in {DbPath}", dbPath);
                _updater.UpdateFromScratch();
                _updater.SetLatestVersion();
            }
            else
            {
                var latestVer = _updater.GetLatestVersion();
                var currentVer = _versionManager.GetCurrentDbVersion();

                if (_versionManager.IsUpToDate(latestVer) == false)
                {
                    _logger.LogWarning("Database V.{CurrentVer} is out of date. Updating to V.{LatestVer}", currentVer, latestVer);
                    _updater.UpdateFrom(currentVer);
                    _updater.SetLatestVersion();
                }
                else { _logger.LogInformation("Database V.{CurrentVer} is up to date. Latest script version is V.{LatestVer}", currentVer, latestVer); }
            }
        }

        #endregion Methods
    }
}