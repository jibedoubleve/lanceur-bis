using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Constants;

public static class AppPaths
{
    public static string Settings            => @"%appdata%\probel\lanceur2\settings.json".ExpandPath();
    public static string PluginUninstallLogs => @"%appdata%\probel\lanceur2\.plugin-uninstall".ExpandPath();
    public static string DefaultDbPath       => @"%appdata%\probel\lanceur2\data.sqlite".ExpandPath();
    public static string ImageCache          => @"%appdata%\probel\lanceur2\thumbnails".ExpandPath();
    
    public const string FaviconPrefix       = "favicon_";
}