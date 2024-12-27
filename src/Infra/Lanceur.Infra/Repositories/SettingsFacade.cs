using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Repositories;

public class SettingsFacade : ISettingsFacade
{
    #region Fields

    private readonly IAppConfigRepository _appConfigRepository;
    private readonly ILocalConfigRepository _localConfigRepository;

    #endregion Fields

    #region Constructors

    public SettingsFacade(ILocalConfigRepository localConfigRepository, IAppConfigRepository appConfigRepository)
    {
        _localConfigRepository = localConfigRepository;
        _appConfigRepository = appConfigRepository;
    }

    #endregion Constructors

    #region Properties

    public AppConfig Application => _appConfigRepository.Current;
    public ILocalConfig Local => _localConfigRepository.Current;

    #endregion Properties

    #region Methods

    public void Save()
    {
        _localConfigRepository.Save();
        _appConfigRepository.Save();
    }

    public void Reload()
    {
        _localConfigRepository.Load();
        _appConfigRepository.Load();
    }

    #endregion Methods
}