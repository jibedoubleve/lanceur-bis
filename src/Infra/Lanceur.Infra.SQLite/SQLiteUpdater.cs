using System.Data;
using System.Reflection;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite;

/// <inheritdoc />
public sealed class SQLiteUpdater : IDatabaseUpdater
{
    #region Fields

    private readonly ILogger<SQLiteUpdater> _logger;
    private readonly SQLiteDatabaseUpdateManager _updater;
    private readonly IDataStoreVersionService _versionService;

    #endregion

    #region Constructors

    public SQLiteUpdater(
        IDataStoreVersionService versionService,
        ILoggerFactory logFactory,
        IDbConnection dbConnection,
        Assembly assembly,
        string pattern
    )
    {
        _versionService = versionService;
        _logger = logFactory.CreateLogger<SQLiteUpdater>();
        _updater = new SQLiteDatabaseUpdateManager(
            versionService,
            dbConnection,
            assembly,
            pattern
        );
    }

    #endregion

    #region Methods

    private static void CreateDirectory(string dbPath)
    {
        var dir = dbPath.GetDirectoryName();
        if (dir is null)
        {
            throw new DirectoryNotFoundException(
                $"Failed to create the directory because the provided path is invalid or null. Path: [{dbPath}]"
            );
        }

        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
    }


    /// <inheritdoc />
    public void Update(string connectionString)
    {
        var dbPath = connectionString.ExtractPathFromSQLiteCString();
        if (!File.Exists(dbPath))
        {
            CreateDirectory(dbPath);
            _logger.LogInformation("Creating a new database in {DbPath}", dbPath);
            _updater.UpdateFromScratch();
            _updater.SetLatestVersion();
        }
        else
        {
            var latestVer = _updater.GetLatestVersion();
            var currentVer = _versionService.GetCurrentDbVersion();

            if (!_versionService.IsUpToDate(latestVer))
            {
                _logger.LogWarning(
                    "Database V.{CurrentVer} is out of date. Updating to V.{LatestVer}",
                    currentVer,
                    latestVer
                );
                _updater.UpdateFrom(currentVer);
                _updater.SetLatestVersion();
            }
            else
            {
                _logger.LogInformation(
                    "Database V.{CurrentVer} is up to date. Latest script version is V.{LatestVer}",
                    currentVer,
                    latestVer
                );
            }
        }
    }

    #endregion
}