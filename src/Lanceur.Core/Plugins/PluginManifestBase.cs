namespace Lanceur.Core.Plugins
{
    public abstract class PluginManifestBase
    {
        #region Properties

        public string Description { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }

        #endregion Properties
    }
}