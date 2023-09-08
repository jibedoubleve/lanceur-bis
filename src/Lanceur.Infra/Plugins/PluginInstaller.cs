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
        private readonly IPluginValidationRule<PluginValidationResult, PluginManifest> _pluginValidationRule;

        #endregion Fields

        #region Constructors

        public PluginInstaller(IAppLoggerFactory appLoggerFactory, IPluginValidationRule<PluginValidationResult, PluginManifest> pluginValidationRule)
        {
            ArgumentNullException.ThrowIfNull(appLoggerFactory, nameof(appLoggerFactory));
            ArgumentNullException.ThrowIfNull(pluginValidationRule, nameof(pluginValidationRule));

            _pluginValidationRule = pluginValidationRule;
            _log = appLoggerFactory.GetLogger<PluginInstaller>();
        }

        #endregion Constructors

        #region Methods

        private static void InstallFiles(string destination, ZipArchive zip)
        {
            ArgumentNullException.ThrowIfNull(destination);
            ArgumentNullException.ThrowIfNull(zip);

            foreach (var entry in zip.Entries)
            {
                // Gets the full path to ensure that relative segments are removed.
                var destinationPath = Path.GetFullPath(Path.Combine(destination, entry.FullName));

                var directory = Path.GetDirectoryName(destinationPath) ?? throw new DirectoryNotFoundException($"No directory found at '{destination}'.");

                if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                entry.ExtractToFile(destinationPath);
            }
        }

        public PluginInstallationResult Install(string packagePath)
        {
            using var zip = ZipFile.OpenRead(packagePath);
            var config = (from entry in zip.Entries
                          where entry.Name == PluginLocation.ManifestName
                          select entry).SingleOrDefault();

            if (config == null)
            {
                _log.Warning($"No plugin manifest in package '{packagePath}'");
                return null;
            }

            using var stream = config.Open();
            using var reader = new StreamReader(stream);

            var json = reader.ReadToEnd();
            var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);

            // Check here whether the plugin already exists. If yes but version
            // is equal or higher, log a warning and stop installation
            var validation = _pluginValidationRule.Check(manifest);
            if (!validation.IsValid)
            {
                _log.Warning(validation.Message);
                return PluginInstallationResult.Error(validation.Message);
            }

            InstallFiles(
                new PluginLocation(manifest).DirectoryPath,
                zip);

            return PluginInstallationResult.Success(manifest);
        }

        public async Task<PluginInstallationResult> InstallFromWebAsync(string url)
        {
            url = PluginWebManifestMetadata.ToAbsoluteUrl(url);
            var path = Path.GetTempFileName();

            // Download & copy to temp directory
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(url))
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                await response.Content.CopyToAsync(stream);
            }

            return Install(path);
        }

        #endregion Methods
    }
}