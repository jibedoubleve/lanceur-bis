using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Repositories;

public class ConfigurationFacadeService : IConfigurationFacade
{
    #region Fields

    private readonly IApplicationSettingsProvider _applicationSettingsProvider;

    private readonly IInfrastructureSettingsProvider _infrastructureSettingsProvider;

    #endregion

    #region Constructors

    public ConfigurationFacadeService(
        IInfrastructureSettingsProvider infrastructureSettingsProvider,
        IApplicationSettingsProvider applicationSettingsProvider
    )
    {
        _infrastructureSettingsProvider = infrastructureSettingsProvider;
        _applicationSettingsProvider = applicationSettingsProvider;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public ApplicationSettings Application => _applicationSettingsProvider.Current;

    /// <inheritdoc />
    public InfrastructureSettings Local => _infrastructureSettingsProvider.Current;

    #endregion

    #region Methods

    private void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc />
    public void Reload()
    {
        _infrastructureSettingsProvider.Load();
        _applicationSettingsProvider.Load();
        OnUpdated();
    }

    /// <inheritdoc />
    public void Save()
    {
        _infrastructureSettingsProvider.Save();
        _applicationSettingsProvider.Save();

        Reload();
    }

    #endregion

    /// <inheritdoc />
    public event EventHandler Updated;
}