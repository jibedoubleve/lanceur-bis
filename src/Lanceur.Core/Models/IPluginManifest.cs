namespace Lanceur.Core.Models
{
    public interface IPluginManifest
    {
        Version AppMinVersion { get; set; }
        string Author { get; set; }
        string Description { get; set; }
        string Dll { get; set; }
        string Help { get; set; }
        string Name { get; set; }
        Version PluginVersion { get; set; }
    }
}