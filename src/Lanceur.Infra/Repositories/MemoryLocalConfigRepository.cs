using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Services;
using Microsoft.Extensions.Logging;
using ILogger = Splat.ILogger;

namespace Lanceur.Infra.Repositories;

public class MemoryLocalConfigRepository : ILocalConfigRepository
{

    #region Fields

    private static readonly LocalConfig Settings;
    private readonly ILogger<MemoryLocalConfigRepository> _logger;

    #endregion Fields

    #region Constructors

    public MemoryLocalConfigRepository(ILogger<MemoryLocalConfigRepository> logger) { _logger = logger; }
    static MemoryLocalConfigRepository()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, "debug.sqlite");

        Settings = new() { DbPath = path };
    }

    #endregion Constructors

    #region Properties

    public ILocalConfig Current => Settings;

    #endregion Properties

    #region Methods

    public void Load()
    {
        /*Does nothing, settings is already in memory*/
    }

    public void Save()
    {
        /*Does nothing, settings is already in memory*/
        _logger.LogWarning("This is a mock service for development purposes. It simulates saving the settings. Current settings: {@Settings}", Settings);
    }

    #endregion Methods
}