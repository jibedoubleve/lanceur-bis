using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Constants;

public static class AppPaths
{
    #region Fields

    public const string FaviconPrefix = "favicon_";

    #endregion Fields

    #region Properties

    public static string DefaultDbPath { get; } = @"%appdata%\probel\lanceur2\data.sqlite".ExpandPath();
    public static string ImageRepository { get; } = @"%appdata%\probel\lanceur2\thumbnails".ExpandPath();
    public static string LogFilePath { get; } = @"%appdata%\probel\lanceur2\logs\probel-lanceur..clef".ExpandPath();
    public static string PluginUninstallLogs { get; } = @"%appdata%\probel\lanceur2\.plugin-uninstall".ExpandPath();
    public static string Settings { get; } = @"%appdata%\probel\lanceur2\settings.json".ExpandPath();

    #endregion Properties
}