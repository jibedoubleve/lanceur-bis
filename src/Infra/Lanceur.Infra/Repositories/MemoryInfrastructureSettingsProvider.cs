using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories.Config;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Repositories;

public class MemoryInfrastructureSettingsProvider : IInfrastructureSettingsProvider
{
    #region Fields

    private readonly ILogger<MemoryInfrastructureSettingsProvider> _logger;

    private static readonly InfrastructureSettings Configuration = new()
    {
        DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            @"lanceur\debug.sqlite"
        )
    };

    #endregion

    #region Constructors

    public MemoryInfrastructureSettingsProvider(ILogger<MemoryInfrastructureSettingsProvider> logger)
        => _logger = logger;

    #endregion

    #region Properties

    public InfrastructureSettings Current => Configuration;

    #endregion

    #region Methods

    public void Load()
    {
        /*Does nothing, settings is already in memory*/
    }

    public void Save()
    {
        /*Does nothing, settings is already in memory*/
        _logger.LogWarning(
            "This is a mock service for development purposes. It simulates saving the settings. Debug database: {Settings}",
            Configuration.DbPath
        );
    }

    #endregion
}