using Lanceur.Core.Models.Settings;

namespace Lanceur.Core.Repositories.Config;

public interface ISettingsFacade
{
    #region Properties

    AppConfig Application { get; }
    ILocalConfig Local { get; }

    #endregion Properties

    #region Methods

    void Save();

    #endregion Methods
}