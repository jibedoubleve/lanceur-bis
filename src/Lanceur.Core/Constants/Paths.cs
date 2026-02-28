using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Constants;

public static class Paths
{
    #region Fields

    public const string GithubUrl =  "https://github.com/jibedoubleve/lanceur-bis";

    #endregion

    #region Properties

    public static string ClefLogFile { get; } = @"%appdata%\probel\lanceur2\logs\clef\lanceur..clef".ExpandPath();

    public static string DebugClefLogFile { get; }
        = @"%appdata%\probel\lanceur2\logs\debug\clef\lanceur..clef".ExpandPath();

    public static string DebugRawLogFile { get; }
        = @"%appdata%\probel\lanceur2\logs\debug\log\lanceur..log".ExpandPath();

    public static string DefaultDb { get; } = @"%appdata%\probel\lanceur2\data.sqlite".ExpandPath();
    public static string ImageRepository { get; } = @"%appdata%\probel\lanceur2\thumbnails".ExpandPath();
    public static string LogRepository { get; } = @"%appdata%\probel\lanceur2\logs".ExpandPath();
    public static string RawLogFile { get; } = @"%appdata%\probel\lanceur2\logs\log\lanceur..log".ExpandPath();
    public static string ReleasesUrl { get; } = $"{GithubUrl}/releases/latest";
    public static string Settings { get; } = @"%appdata%\probel\lanceur2\settings.json".ExpandPath();

    #endregion
}