using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Repositories;

public class SettingsFacadeService : ISettingsFacade
{
    #region Fields

    private readonly IApplicationConfigurationService _applicationConfigurationService;

    private readonly IDatabaseConfigurationService _databaseConfigurationService;

    #endregion

    #region Constructors

    public SettingsFacadeService(
        IApplicationConfigurationService applicationConfigurationService,
        IDatabaseConfigurationService databaseConfigurationService
    )
    {
        _applicationConfigurationService = applicationConfigurationService;
        _databaseConfigurationService = databaseConfigurationService;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public DatabaseConfiguration Application => _databaseConfigurationService.Current;

    /// <inheritdoc />
    public ApplicationConfiguration Local => _applicationConfigurationService.Current;

    #endregion

    #region Methods

    private void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc />
    public void Reload()
    {
        _applicationConfigurationService.Load();
        _databaseConfigurationService.Load();
    }

    /// <inheritdoc />
    public void Save()
    {
        _applicationConfigurationService.Save();
        _databaseConfigurationService.Save();

        OnUpdated();
        Reload();
    }

    #endregion

    /// <inheritdoc />
    public event EventHandler Updated;
}