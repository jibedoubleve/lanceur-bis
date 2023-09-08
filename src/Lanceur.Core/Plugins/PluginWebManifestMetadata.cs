namespace Lanceur.Core.Plugins;

public struct PluginWebManifestMetadata
{
    #region Fields

    public const string BaseUrl =
        "https://raw.githubusercontent.com/jibedoubleve/lanceur-bis-plugin-repository/master/";

    public const string UrlToc = $"{BaseUrl}toc.json";

    #endregion Fields

    #region Methods

    public static string ToAbsoluteUrl(string relativeUrl)
    {
        if (!relativeUrl.Trim('/').StartsWith("plugin"))
        {
            relativeUrl = $"plugins/{relativeUrl.Trim('/')}";
        }

        return $"{BaseUrl}{relativeUrl}";
    }

    #endregion Methods
}
