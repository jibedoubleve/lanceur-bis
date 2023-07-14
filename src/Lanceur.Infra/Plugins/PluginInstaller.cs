using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Newtonsoft.Json;
using System.IO.Compression;

namespace Lanceur.Infra.Plugins
{
    public class PluginInstaller : IPluginInstaller
    {
        #region Fields

        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public PluginInstaller(IAppLoggerFactory appLoggerFactory)
        {
            ArgumentNullException.ThrowIfNull(appLoggerFactory, nameof(appLoggerFactory));

            _log = appLoggerFactory.GetLogger<PluginInstaller>();
        }

        #endregion Constructors

        #region Methods

        private static string GetExtractPath(PluginConfiguration manifest)
        {
            return Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            @"lanceur2/Plugins",
                            Path.GetDirectoryName(manifest.Dll).Trim('\\')
                        );
        }

        private static void InstallFiles(ZipArchive zip, string extractPath)
        {
            foreach (var entry in zip.Entries)
            {
                // Gets the full path to ensure that relative segments are removed.
                string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                var directory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                entry.ExtractToFile(destinationPath);
            }
        }

        public IPluginConfiguration Install(string packagePath)
        {
            using var zip = ZipFile.OpenRead(packagePath);
            var config = (from entry in zip.Entries
                          where entry.Name == "plugin.config.json"
                          select entry).SingleOrDefault();

            if (config == null)
            {
                _log.Warning($"No plugin manifest in package '{packagePath}'");
                return null;
            }

            using var stream = config.Open();
            using var reader = new StreamReader(stream);

            var json = reader.ReadToEnd();
            var manifest = JsonConvert.DeserializeObject<PluginConfiguration>(json);

            var extractPath = GetExtractPath(manifest);
            InstallFiles(zip, extractPath);

            return manifest;
        }

        #endregion Methods
    }
}