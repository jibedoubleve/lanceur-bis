using Lanceur.Core.Models;

namespace Lanceur.Core.Plugins
{
    public interface IPluginUninstaller
    {
        #region Methods

        Task<IEnumerable<UninstallCandidate>> GetCandidatesAsync();

        Task SubscribeForUninstallAsync(IPluginConfiguration pluginConfiguration);

        Task UninstallAsync();

        bool HasCandidateForUninstall();

        #endregion Methods
    }
}