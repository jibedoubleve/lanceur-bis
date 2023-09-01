namespace Lanceur.Core.Plugins;

public struct PluginWebManifestMetadata
{
    #region Fields

    public const string BaseUrl =
        "https://raw.githubusercontent.com/jibedoubleve/lanceur-bis-plugin-repository/master/";

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

    #endregion Methods
}
