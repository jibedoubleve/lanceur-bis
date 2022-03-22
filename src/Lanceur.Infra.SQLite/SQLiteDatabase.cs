using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.SQLite
{
    public class SQLiteDatabase
    {
        #region Fields

        private readonly ILogService _log;
        private readonly IDataStoreUpdateManager _updater;
        private readonly IDataStoreVersionManager _versionManager;

        #endregion Fields

        #region Constructors

        public SQLiteDatabase(
            IDataStoreVersionManager versionManager,
            ILogService logService,
            IDataStoreUpdateManager updater
            )
        {
            _versionManager = versionManager;
            _log = logService;
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
                _log.Warning($"Creating a new database in '{dbPath}'");
                _updater.UpdateFromScractch();
                _updater.SetLatestVersion();
            }
            else
            {
                var latestVer = _updater.GetLatestVersion();
                var currentVer = _versionManager.GetCurrentDbVersion();

                if (_versionManager.IsUpToDate(latestVer) == false)
                {
                    _log.Warning($"Database V.{currentVer} is out of date. Updating to V.{latestVer}");
                    _updater.UpdateFrom(currentVer);
                    _updater.SetLatestVersion();
                }
                else { _log.Info($"Database V.{currentVer} is up to date. Latest script version is V.{latestVer}"); }
            }
        }

        #endregion Methods
    }
}