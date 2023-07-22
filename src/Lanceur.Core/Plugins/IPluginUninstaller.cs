using Lanceur.Core.Models;

namespace Lanceur.Core.Plugins
{
    public interface IPluginUninstaller
    {
        #region Methods

        Task<IEnumerable<UninstallCandidate>> GetCandidatesAsync();

        Task SubscribeForUninstallAsync(IPluginManifest pluginManifest);

        Task UninstallAsync();

        bool HasCandidateForUninstall();

        #endregion Methods
    }
}