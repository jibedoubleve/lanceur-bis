namespace Lanceur.SharedKernel.Extensions;

public static class PathExtensions
{
    #region Methods

    public static string ExpandPath(this string path) => Environment.ExpandEnvironmentVariables(path);

    public static string GetDirectoryName(this string path) => Path.GetDirectoryName(path);

    #endregion
}