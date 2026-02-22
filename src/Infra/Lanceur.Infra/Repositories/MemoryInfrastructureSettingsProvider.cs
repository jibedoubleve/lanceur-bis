using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Repositories;

public class MemoryInfrastructureSettingsProvider : IInfrastructureSettingsProvider
{
    #region Fields

    private static readonly InfrastructureSettings Configuration = new()
    {
        DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            @"lanceur\debug.sqlite"
        )
    };

    #endregion

    #region Properties

    public InfrastructureSettings Current => Configuration;

    #endregion

    #region Methods

    public void Load()
    {
        // Does nothing, settings is already in memory
    }

    public void Save()
    {
        // Does nothing, settings is already in memory
    }

    #endregion
}