﻿using System.Data;
using Lanceur.Core.Managers;
using System.Data.SQLite;
using System.Reflection;
using System.SQLite.Updater;

namespace Lanceur.Infra.SQLite;

public class SQLiteDatabaseUpdateManager : IDataStoreUpdateManager
{
    #region Fields

    private readonly DatabaseUpdater _dbUpdater;
    private readonly IDataStoreVersionManager _versionManager;

    #endregion Fields

    #region Constructors

    public SQLiteDatabaseUpdateManager(IDataStoreVersionManager versionManager, IDbConnection db, Assembly asm, string pattern)
    {
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