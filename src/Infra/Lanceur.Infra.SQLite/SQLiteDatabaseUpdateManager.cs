using System.Data;
using Lanceur.Core.Managers;
using System.Data.SQLite;
using System.Reflection;
using System.SQLite.Updater;

namespace Lanceur.Infra.SQLite;

internal class SQLiteDatabaseUpdateManager 
{
    #region Fields

    private readonly DatabaseUpdater _dbUpdater;
    private readonly IDataStoreVersionManager _versionManager;

    #endregion Fields

    #region Constructors

    public SQLiteDatabaseUpdateManager(IDataStoreVersionManager versionManager, IDbConnection db, Assembly asm, string pattern)
    {
        ArgumentNullException.ThrowIfNull(versionManager);
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(asm);

        if (string.IsNullOrEmpty(db.ConnectionString)) throw new ArgumentNullException(nameof(db.ConnectionString), "ConnectionString should have a value");

        _dbUpdater = new(db, asm, pattern);
        _versionManager = versionManager;
    }

    #endregion Constructors

    #region Methods

    public Version GetLatestVersion() => _dbUpdater.MaxVersion();

    public void SetLatestVersion() => _versionManager.SetCurrentDbVersion(_dbUpdater.MaxVersion());

    public void UpdateFrom(string version) => UpdateFrom(new Version(version));

    public void UpdateFrom(Version version) => _dbUpdater.UpdateFrom(version);

    public void UpdateFromScratch() => _dbUpdater.UpdateFromScratch();

    #endregion Methods
}