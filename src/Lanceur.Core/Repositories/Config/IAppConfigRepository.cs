using Lanceur.Core.Models.Settings;

namespace Lanceur.Core.Repositories.Config;

public interface IAppConfigRepository : IConfigRepository<AppConfig>
{
    #region Methods

    void Edit(Action<AppConfig> action);

    #endregion Methods
}