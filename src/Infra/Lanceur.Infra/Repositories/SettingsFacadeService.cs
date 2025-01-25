using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Repositories;

public class SettingsFacadeService : ISettingsFacade
{
    #region Fields

    private readonly IDatabaseConfigurationService _databaseConfigurationService;
    private readonly IApplicationConfigurationService _applicationConfigurationService;

    #endregion Fields

    #region Constructors

    public SettingsFacadeService(IApplicationConfigurationService applicationConfigurationService, IDatabaseConfigurationService databaseConfigurationService)
    {
        _applicationConfigurationService = applicationConfigurationService;
        _databaseConfigurationService = databaseConfigurationService;
    }

    #endregion Constructors

    #region Properties

    public DatabaseConfiguration Application => _databaseConfigurationService.Current;
    public IApplicationSettings Local => _applicationConfigurationService.Current;

    #endregion Properties

    #region Methods

    public void Save()
    {
        _applicationConfigurationService.Save();
        _databaseConfigurationService.Save();
        
        Reload();
    }

    public void Reload()
    {
        _applicationConfigurationService.Load();
        _databaseConfigurationService.Load();
    }

    #endregion Methods
}