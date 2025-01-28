using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Constants;

public static class Paths
{
    #region Properties

    public static string DefaultDb { get; } = @"%appdata%\probel\lanceur2\data.sqlite".ExpandPath();
    public static string ImageRepository { get; } = @"%appdata%\probel\lanceur2\thumbnails".ExpandPath();
    public static string LogFile { get; } = @"%appdata%\probel\lanceur2\logs\probel-lanceur..clef".ExpandPath();
    public static string PluginUninstallLogs { get; } = @"%appdata%\probel\lanceur2\.plugin-uninstall".ExpandPath();
    public static string Settings { get; } = @"%appdata%\probel\lanceur2\settings.json".ExpandPath();
    public static string TelemetryUrl => "http://localhost:5341";

    #endregion
}