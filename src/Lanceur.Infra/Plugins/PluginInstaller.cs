using FileOperationScheduler.Infrastructure.Operations;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;
using System.IO.Compression;

namespace Lanceur.Infra.Plugins
{
    public class PluginInstaller : PluginButler, IPluginInstaller
    {
        #region Fields

        private readonly IAppLogger _log;
        private readonly IPluginUninstaller _pluginUninstaller;
        private readonly IPluginValidationRule<PluginValidationResult, PluginManifest> _pluginValidationRule;

        #endregion Fields

        #region Constructors

        public PluginInstaller(
            IAppLoggerFactory appLoggerFactory,
            IPluginValidationRule<PluginValidationResult, PluginManifest> pluginValidationRule,
            IPluginUninstaller pluginUninstaller)
        {
            ArgumentNullException.ThrowIfNull(appLoggerFactory, nameof(appLoggerFactory));
            ArgumentNullException.ThrowIfNull(pluginValidationRule, nameof(pluginValidationRule));

            _pluginValidationRule = pluginValidationRule;
            _pluginUninstaller = pluginUninstaller;
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

                var directory = Path.GetDirectoryName(destinationPath) ??
                                throw new DirectoryNotFoundException($"No directory found at '{destination}'.");

                if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                entry.ExtractToFile(destinationPath);
            }
        }

        public async Task<bool> HasMaintenanceAsync()
        {
            var os = await GetOperationSchedulerAsync();
            return os.GetState().OperationCount > 0;
        }

        public async Task<PluginInstallationResult> InstallAsync(string packagePath)
        {
            using var zip = ZipFile.OpenRead(packagePath);
            var config = (from entry in zip.Entries
                          where entry.Name == Locations.ManifestFileName
                          select entry).SingleOrDefault();

            if (config == null)
            {
                _log.Warning($"No plugin manifest in package '{packagePath}'");
                return null;
            }

            await using var stream = config.Open();
            using var reader = new StreamReader(stream);

            var json = await reader.ReadToEndAsync();
            var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);
            RemoveCandidate(manifest);

            // Check here whether the plugin already exists. If yes but version
            // is equal or higher, log a warning and stop installation
            var validation = _pluginValidationRule.Check(manifest);
            if (!validation.IsValid)
            {
                _log.Warning(validation.Message);
                return PluginInstallationResult.Error(validation.Message);
            }

            if (validation.IsUpdate)
            {
                // If it is an update, the plugin to be updated  is in use.
                // A restart is then needed. Put the info in the maintenance
                // log and display the Plugin in the list of plugins.
                // Restart will really install it.

                await _pluginUninstaller.SubscribeForUninstallAsync(manifest);

                var tempPath = FileHelper.GetRandomTempFile(".zip");
                File.Copy(packagePath, tempPath);

                var os = await GetOperationSchedulerAsync();
                await os.AddOperation(OperationFactory.UnzipDirectory(tempPath, Locations.FromManifest(manifest).PluginDirectoryPath))
                        .SavePlanAsync();

                return PluginInstallationResult.Success(manifest, isUpdate: true);
            }

            InstallFiles(
                Locations.FromManifest(manifest).PluginDirectoryPath, zip);
            return PluginInstallationResult.Success(manifest);
        }

        public async Task<string> InstallAsync()
        {
            var os = await GetOperationSchedulerAsync();
            await os.ExecutePlanAsync();
            return string.Empty;
        }

        public async Task<PluginInstallationResult> InstallFromWebAsync(string url)
        {
            url = PluginWebManifestMetadata.ToAbsoluteUrl(url);
            var path = Path.GetTempFileName();

            // Download & copy to temp directory
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(url))
            await using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                await response.Content.CopyToAsync(stream);
            }

            return await InstallAsync(path);
        }

        #endregion Methods
    }
}