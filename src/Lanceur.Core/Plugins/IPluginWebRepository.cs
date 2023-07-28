using Lanceur.Core.Plugins;

namespace Lanceur.Core.Plugins
{
    public interface IPluginWebRepository
    {
        Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync();
    }
}