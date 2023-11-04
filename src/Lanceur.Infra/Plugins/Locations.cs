using Lanceur.Core.Plugins;
using Lanceur.Infra.Constants;
using IoDirectory = System.IO.Directory;

namespace Lanceur.Infra.Plugins
{
    public class Locations
    {
        #region Fields

        private static readonly char[] TrimChars = { '\\', '/' };
        public const string ManifestFileName = "manifest.json";
        public static readonly string RelativePath = Path.Combine("lanceur2", "Plugins");

        #endregion Fields

        #region Constructors

        private Locations(string relativePath)
        {
            var absolutePath = Path.GetFullPath(Path.Combine(PluginRootPath, relativePath));

            PluginDirectoryPath = Path.GetDirectoryName(absolutePath);
            PluginDllPath = absolutePath;
        }

        private Locations(IPluginManifestBase manifest)
        {
            PluginDirectoryPath = Path.Combine(
                PluginRootPath,
                Path.GetDirectoryName(manifest.Dll)?.Trim(TrimChars) ?? ""
            );
            PluginDllPath = Path.Combine(PluginDirectoryPath, new FileInfo(manifest.Dll).Name);
        }

        static Locations()
        {
            PluginRootPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RelativePath
            );

            MaintenanceLogBookPath = Environment.ExpandEnvironmentVariables(AppPaths.PluginUninstallLogs);
        }

        #endregion Constructors

        #region Properties

        public static string MaintenanceLogBookPath { get; }

        public static string PluginRootPath { get; }

        public DirectoryInfo PluginDirectory => new(PluginDirectoryPath);

        public string PluginDirectoryPath { get; }

        public string PluginDllPath { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Get the absolute path for plugins based on the specified
        /// directory relative path
        /// </summary>
        /// <param name="path">Relative path of the plugins repository</param>
        /// <returns>An instance of <see cref="Locations"/> that specifies all the locations</returns>
        public static Locations FromDirectory(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            path = path.Trim(TrimChars) + "\\";
            return new(path);
        }

        /// <summary>
        /// Get the absolute path for plugins based on the specified
        /// relative path
        /// </summary>
        /// <param name="path">Relative path of the plugins repository</param>
        /// <returns>An instance of <see cref="Locations"/> that specifies all the locations</returns>
        public static Locations FromFile(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            path = path.TrimStart(TrimChars);
            return new(path);
        }

        /// <summary>
        /// Infer plugin path based on the specified manifest
        /// </summary>
        /// <param name="manifest">The plugin manifest</param>
        /// <returns>An instance of <see cref="Locations"/> that specifies all the locations</returns>
        public static Locations FromManifest(IPluginManifest manifest) => new(manifest);

        public static string GetAbsolutePath(string path) => Path.Combine(PluginRootPath, path.Trim('\\', '/'));

        /// <summary>
        ///     Deletes recursively the plugin directories
        /// </summary>
        public void Delete() => IoDirectory.Delete(PluginDirectoryPath, true);

        /// <summary>
        ///     Checks whether the plugin directory exists
        /// </summary>
        /// <returns><c>True</c> if plugin directory exists, otherwise <c>False</c></returns>
        public bool Exists() => IoDirectory.Exists(PluginDirectoryPath);

        #endregion Methods
    }
}