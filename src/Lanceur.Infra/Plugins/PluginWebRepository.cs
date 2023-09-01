using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins
{
    public class PluginWebRepository : IPluginWebRepository
    {
        #region Fields

        private readonly IPluginManifestRepository _localPluginManifest;
        private readonly IPluginWebManifestLoader _webPluginManifest;

        #endregion Fields

        public PluginWebRepository(IPluginManifestRepository localPluginManifest,
            IPluginWebManifestLoader webPluginManifest)
        {
            _localPluginManifest = localPluginManifest;
            _webPluginManifest = webPluginManifest;
        }

        #region Methods

        public async Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync()
        {
            var webPlugins = await _webPluginManifest.LoadFromWebAsync();
            var plugins = _localPluginManifest.GetPluginManifests();
            
            // If a plugin is already installed but web version is 
            // newer, then show this new plugin
            return (from p in webPlugins
                where false == plugins.Any(
                    x => x.Dll == p.Dll 
                         && x.Version >= p.Version
                )
                select p).ToArray();
        }

        #endregion Methods
    }
}