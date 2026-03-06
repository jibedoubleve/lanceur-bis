namespace Lanceur.SharedKernel.Extensions;

public static class PathExtensions
{
    #region Methods

    /// <summary>
    ///     Replaces environment variable placeholders (e.g. <c>%APPDATA%</c>) in the path with their values.
    /// </summary>
    public static string ExpandPath(this string path) => Environment.ExpandEnvironmentVariables(path);

    /// <summary>
    ///     Returns the directory component of the path, or <c>null</c> if the path has no directory component
    ///     (e.g. a root or empty path).
    /// </summary>
    public static string? GetDirectoryName(this string path) => Path.GetDirectoryName(path);

    #endregion
}