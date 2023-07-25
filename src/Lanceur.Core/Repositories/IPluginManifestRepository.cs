using Lanceur.Core.Models;

namespace Lanceur.Core.Repositories
{
    public interface IPluginManifestRepository
    {
        #region Methods

        /// <summary>
        /// Get the configuration of all plugins installed
        /// </summary>
        /// <returns>The list of all configuration</returns>
        IPluginManifest[] GetPluginManifests();

        #endregion Methods
    }
}