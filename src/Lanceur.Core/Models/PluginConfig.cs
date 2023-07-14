namespace Lanceur.Core.Models
{
    /// <summary>
    /// Represents the configuration of a plugin
    /// </summary>
    public class PluginConfiguration
    {
        #region Properties

        public Version AppMinVersion { get; set; }

        public string Author { get; set; }
        public string Description { get; set; }
        public string Dll { get; set; }
        public string Help { get; set; }
        public string Name { get; set; }

        public Version PluginVersion { get; set; }

        #endregion Properties
    }
}