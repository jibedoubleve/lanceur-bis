using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Constants;

public static class AppPaths
{
    #region Fields

    public const string FaviconPrefix = "favicon_";

    #endregion Fields

    #region Properties

    public static string DefaultDbPath => @"%appdata%\probel\lanceur2\data.sqlite".ExpandPath();
    public static string ImageRepository => @"%appdata%\probel\lanceur2\thumbnails".ExpandPath();
    public static string LogFilePath => @"%appdata%\probel\lanceur2\logs\probel-lanceur..clef".ExpandPath();
    public static string PluginUninstallLogs => @"%appdata%\probel\lanceur2\.plugin-uninstall".ExpandPath();
    public static string Settings => @"%appdata%\probel\lanceur2\settings.json".ExpandPath();

    #endregion Properties
}