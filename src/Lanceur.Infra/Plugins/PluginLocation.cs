using Lanceur.Core.Plugins;
using IoDirectory = System.IO.Directory;

namespace Lanceur.Infra.Plugins
{
    public class PluginLocation
    {
        #region Fields

        private static readonly char[] TRIM_CHARS = new char[] { '\\', '/' };
        public const string ManifestName = "manifest.json";
        public static readonly string RelativePath = Path.Combine("lanceur2", "Plugins");

        #endregion Fields

        #region Constructors

        private PluginLocation(string path)
        {
            var fullPath = Path.GetFullPath(Path.Combine(Root, path));

            DirectoryPath = Path.GetDirectoryName(fullPath);
            FullPath = fullPath;
        }

        static PluginLocation()
        {
            Root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RelativePath
            );

            MaintenanceLogBook = Environment.ExpandEnvironmentVariables(@"%appdata%/probel/lanceur2/.plugin-uninstall");
        }

        public PluginLocation(IPluginManifest manifest)
        {
            DirectoryPath = Path.Combine(
                Root,
                Path.GetDirectoryName(manifest.Dll).Trim(TRIM_CHARS)
            );
            FullPath = Path.Combine(DirectoryPath, new FileInfo(manifest.Dll).Name);
        }

        #endregion Constructors

        #region Properties

        public static string Root { get; }

        public static string MaintenanceLogBook { get; }
        public DirectoryInfo Directory => new DirectoryInfo(DirectoryPath);

        public string DirectoryPath { get; }

        public string FullPath { get; }

        #endregion Properties

        #region Methods

        public static PluginLocation FromDirectory(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            path = path.Trim(TRIM_CHARS) + "\\";
            return new(path);
        }

        public static PluginLocation FromFile(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            path = path.TrimStart(TRIM_CHARS);
            return new(path);
        }

        /// <summary>
        /// Deletes recurcively the plugin directories
        /// </summary>
        public void Delete() => IoDirectory.Delete(DirectoryPath, recursive: true);

        /// <summary>
        /// Checks whether the plugin directory exists
        /// </summary>
        /// <returns></returns>
        public bool Exists() => IoDirectory.Exists(DirectoryPath);

        #endregion Methods
    }
}