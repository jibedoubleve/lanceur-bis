using Lanceur.Core.Models;

namespace Lanceur.Core.Plugins
{
    public interface IPluginUninstaller
    {
        #region Methods

        Task SubscribeForUninstallAsync(IPluginConfiguration pluginConfiguration);

        Task UninstallAsync();

        #endregion Methods
    }
}