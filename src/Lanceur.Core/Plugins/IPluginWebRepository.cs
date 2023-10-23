using Lanceur.Core.Plugins;

namespace Lanceur.Core.Plugins
{
    public interface IPluginWebRepository
    {
        /// <summary>
        /// Get all the installed plugins and remove those which are still installed but candidate for a remove
        /// after reboot.
        /// </summary>
        /// <param name="uninstallationCandidates">All the candidate for uninstall after next reboot</param>
        /// <returns>The list of all plugins installed even after the next reboot.</returns>
        Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync(IEnumerable<IPluginManifest> uninstallationCandidates = null);
    }
}