using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;

namespace Lanceur.Infra.Plugins
{
    public class PluginWebRepository : IPluginWebRepository
    {
        #region Fields

        private readonly IPluginManifestRepository _localPluginManifestRepository;
        private readonly IPluginWebManifestLoader _webPluginManifestLoader;

        #endregion Fields

        public PluginWebRepository(
            IPluginManifestRepository localPluginManifestRepository,
            IPluginWebManifestLoader webPluginManifestLoader)
        {
            _localPluginManifestRepository = localPluginManifestRepository;
            _webPluginManifestLoader = webPluginManifestLoader;
        }

        #region Methods

        public async Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync()
        {
            var webPlugins = await _webPluginManifestLoader.LoadFromWebAsync();
            var plugins = _localPluginManifestRepository.GetPluginManifests();
            
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