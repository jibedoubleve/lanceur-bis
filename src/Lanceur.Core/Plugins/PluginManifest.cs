namespace Lanceur.Core.Plugins;

/// <summary>
/// Represents the configuration of a plugin
/// </summary>
public class PluginManifest : PluginManifestBase, IPluginManifest
{
    #region Properties

    public Version AppMinVersion { get; set; }
    public string Author { get; set; }
    public string Dll { get; set; }

    #endregion Properties
}