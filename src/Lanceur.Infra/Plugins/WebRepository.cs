using Lanceur.Core.Plugins;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins
{
    public class WebRepository : IWebRepository
    {
        #region Fields

        public const string BaseUrl = "https://raw.githubusercontent.com/jibedoubleve/lanceur-bis-plugin-repository/master/";
        public const string UrlToc = $"{BaseUrl}toc.json";

        #endregion Fields

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

            return JsonConvert.DeserializeObject<List<PluginWebManifest>>(json);

        }

        #endregion Methods
    }
}