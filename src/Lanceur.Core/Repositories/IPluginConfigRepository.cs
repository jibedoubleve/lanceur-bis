using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories
{
    public interface IPluginConfigRepository
    {
        #region Methods

        /// <summary>
        /// Get the configuration of all plugins installed
        /// </summary>
        /// <returns>The list of all configuration</returns>
        IPluginConfiguration[] GetPluginConfigurations();

        #endregion Methods
    }
}