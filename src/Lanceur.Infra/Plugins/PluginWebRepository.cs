using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins
{
    public class PluginWebRepository : IPluginWebRepository
    {
        #region Fields

        public const string BaseUrl = "https://raw.githubusercontent.com/jibedoubleve/lanceur-bis-plugin-repository/master/";
        public const string UrlToc = $"{BaseUrl}toc.json";
        private readonly IPluginManifestRepository _pluginManifestRepository;

        #endregion Fields

        public PluginWebRepository(IPluginManifestRepository pluginManifestRepository)
        {
            _pluginManifestRepository = pluginManifestRepository;
        }

        #region Methods

        public static string ExpandUrl(string url)
        {
            if (!url.Trim('/').StartsWith("plugin"))
            {
                url = $"plugins/{url.Trim('/')}";
            }

            return $"{BaseUrl}{url}";
        }

        public async Task<IEnumerable<IPluginWebManifest>> GetPluginListAsync()
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(UrlToc);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            var webplugins = JsonConvert.DeserializeObject<List<PluginWebManifest>>(json);
            var plugins = _pluginManifestRepository.GetPluginManifests();

            return (from p in webplugins
                    where false == plugins.Where(x => x.Dll == p.Dll).Any()
                    select p).ToArray();
        }

        #endregion Methods
    }
}