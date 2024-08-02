using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Infra.Constants;

public static class Paths
{
    #region Fields

    private static readonly Conditional<string> TelemetryUrlValue = new("http://localhost:5341", "http://ec2-35-180-121-110.eu-west-3.compute.amazonaws.com:5341");

    #endregion Fields

    #region Properties

    public static string DefaultDb { get; } = @"%appdata%\probel\lanceur2\data.sqlite".ExpandPath();
    public static string ImageRepository { get; } = @"%appdata%\probel\lanceur2\thumbnails".ExpandPath();
    public static string LogFile { get; } = @"%appdata%\probel\lanceur2\logs\probel-lanceur..clef".ExpandPath();
    public static string TelemetryUrl => TelemetryUrlValue;
    public static string PluginUninstallLogs { get; } = @"%appdata%\probel\lanceur2\.plugin-uninstall".ExpandPath();
    public static string Settings { get; } = @"%appdata%\probel\lanceur2\settings.json".ExpandPath();

    #endregion Properties
}