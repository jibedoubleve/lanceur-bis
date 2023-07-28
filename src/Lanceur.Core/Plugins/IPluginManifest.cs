namespace Lanceur.Core.Plugins
{
    public interface IPluginManifest : IPluginManifestBase
    {
        #region Properties

        Version AppMinVersion { get; set; }
        string Author { get; set; }
        string Dll { get; set; }

        #endregion Properties
    }

    public interface IPluginManifestBase
    {
        #region Properties

        string Description { get; set; }
        string Name { get; set; }
        Version Version { get; set; }

        #endregion Properties
    }

    public interface IPluginWebManifest : IPluginManifestBase
    {
        #region Properties

        string Url { get; set; }

        #endregion Properties
    }
}