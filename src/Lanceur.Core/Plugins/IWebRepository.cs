using Lanceur.Core.Plugins;

namespace Lanceur.Core.Plugins
{
    public interface IWebRepository
    {
        Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync();
    }
}