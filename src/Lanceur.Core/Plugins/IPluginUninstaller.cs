using System.Collections.Immutable;

namespace Lanceur.Core.Plugins
{
    public interface IPluginUninstaller
    {
        #region Methods

        /// <summary>
        /// Represents the plugins the user asked to delete
        /// </summary>
        public IEnumerable<IPluginManifest> UninstallationCandidates { get; }
        
        Task<bool> HasMaintenanceAsync();

        Task SubscribeForUninstallAsync(IPluginManifest pluginManifest);
        
        #endregion Methods
    }
}