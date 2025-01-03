using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.SQLite.Updater;
using Lanceur.Core.Services;

namespace Lanceur.Infra.SQLite;

internal class SQLiteDatabaseUpdateManager 
{
    #region Fields

    private readonly DatabaseUpdater _dbUpdater;
    private readonly IDataStoreVersionService _versionService;

    #endregion Fields

    #region Constructors

    public SQLiteDatabaseUpdateManager(IDataStoreVersionService versionService, IDbConnection db, Assembly asm, string pattern)
    {
        ArgumentNullException.ThrowIfNull(versionService);
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(asm);

        if (string.IsNullOrEmpty(db.ConnectionString)) throw new ArgumentNullException(nameof(db.ConnectionString), "ConnectionString should have a value");

        _dbUpdater = new(db, asm, pattern);
        _versionService = versionService;
    }

    #endregion Constructors

    #region Methods

    public Version GetLatestVersion() => _dbUpdater.MaxVersion();

    public void SetLatestVersion() => _versionService.SetCurrentDbVersion(_dbUpdater.MaxVersion());

    public void UpdateFrom(string version) => UpdateFrom(new Version(version));

    public void UpdateFrom(Version version) => _dbUpdater.UpdateFrom(version);

    public void UpdateFromScratch() => _dbUpdater.UpdateFromScratch();

    #endregion Methods
}