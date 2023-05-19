using Lanceur.Core.Models.Settings;

namespace Lanceur.Core.Services.Config
{
    public interface IAppConfigService : IConfigService<AppConfig>
    {
        #region Methods

        void Edit(Action<AppConfig> action);

        #endregion Methods
    }
}