using Lanceur.Core.Configuration;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Repositories;

public class MemoryApplicationConfigurationService : IApplicationConfigurationService
{
    #region Fields

    private readonly ILogger<MemoryApplicationConfigurationService> _logger;

    private static readonly ApplicationSettings Settings;

    #endregion

    #region Constructors

    static MemoryApplicationConfigurationService()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, "debug.sqlite");

        Settings = new() { DbPath = path };
    }

    public MemoryApplicationConfigurationService(ILogger<MemoryApplicationConfigurationService> logger) => _logger = logger;

    #endregion

    #region Properties

    public ApplicationSettings Current => Settings;

    #endregion

    #region Methods

    public void Load()
    {
        /*Does nothing, settings is already in memory*/
    }

    public void Save()
    {
        /*Does nothing, settings is already in memory*/
        _logger.LogWarning("This is a mock service for development purposes. It simulates saving the settings. Debug database: {Settings}", Settings.DbPath);
    }

    #endregion
}