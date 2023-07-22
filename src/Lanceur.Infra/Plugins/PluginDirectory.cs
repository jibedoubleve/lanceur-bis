using Lanceur.Core.Models;
using IoDirectory = System.IO.Directory;
using IoPath = System.IO.Path;

namespace Lanceur.Infra.Plugins
{
    public class PluginDirectory
    {
        #region Fields

        private static readonly char[] TRIM_CHARS = new char[] { '\\', '/' };
        public static readonly string RelativePath = IoPath.Combine("lanceur2", "Plugins");

        #endregion Fields

        #region Constructors

        static PluginDirectory()
        {
            Root = IoPath.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RelativePath
            );

            UninstallManifest = Environment.ExpandEnvironmentVariables(@"%appdata%/probel/lanceur2/.plugin-uninstall");
        }

        public PluginDirectory(string directory)
        {
            Path = IoPath.Combine(Root, directory.Trim(TRIM_CHARS));
        }

        public PluginDirectory(IPluginConfiguration manifest)
        {
            Path = IoPath.Combine(
                Root,
                IoPath.GetDirectoryName(manifest.Dll).Trim(TRIM_CHARS)
            );
        }

        #endregion Constructors

        #region Properties

        public static string Root { get; }
        public static string UninstallManifest { get; }
        public DirectoryInfo Directory => new DirectoryInfo(Path);
        public string Path { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Deletes recurcively the plugin directorys
        /// </summary>
        public void Delete() => IoDirectory.Delete(Path, recursive: true);

        /// <summary>
        /// Checks whether the plugin directory exists
        /// </summary>
        /// <returns></returns>
        public bool Exists() => IoDirectory.Exists(Path);

        #endregion Methods
    }
}