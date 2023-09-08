using Lanceur.Core.Plugins;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins;

public class PluginWebManifestLoader : IPluginWebManifestLoader
{
    #region Methods

    public async Task<IEnumerable<IPluginWebManifest>> LoadFromWebAsync()
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(PluginWebManifestMetadata.UrlToc);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<List<PluginWebManifest>>(json);
    }

    #endregion Methods
}