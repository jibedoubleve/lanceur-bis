namespace Lanceur.SharedKernel.Mixins
{
    public static class PathMixin
    {
        #region Methods

        public static string GetDirectoryName(this string path) => Path.GetDirectoryName(path);

        public static string ExpandPath(this string path) => Environment.ExpandEnvironmentVariables(path);

        #endregion Methods
    }
}