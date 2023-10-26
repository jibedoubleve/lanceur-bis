namespace Lanceur.Core.Plugins
{
    public class PluginWebManifest : PluginManifestBase, IPluginWebManifest
    {
        #region Properties

        public string Dll { get; set; }
        public string Url { get; set; }

        #endregion Properties
    }
}