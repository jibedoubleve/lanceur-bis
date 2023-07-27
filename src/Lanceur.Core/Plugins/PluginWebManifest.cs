namespace Lanceur.Core.Plugins
{
    public class PluginWebManifest : IPluginWebManifest
    {
        #region Properties

        public string Description { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Version Version { get; set; }

        #endregion Properties
    }
}