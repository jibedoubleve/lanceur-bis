namespace Lanceur.Core.Plugins
{
    /// <summary>
    /// Represents the configuration of a plugin
    /// </summary>
    public class PluginManifest : IPluginManifest
    {
        #region Properties

        public Version AppMinVersion { get; set; }

        public string Author { get; set; }
        public string Description { get; set; }
        public string Dll { get; set; }
        public string Help { get; set; }
        public string Name { get; set; }
        public Version Version { get; set; }

        #endregion Properties
    }
}