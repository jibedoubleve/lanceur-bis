using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;
using System.IO.Compression;
using System.IO.FileOps.Core;
using System.IO.FileOps.Infrastructure;
using System.IO.FileOps.Infrastructure.Operations;

namespace Lanceur.Infra.Plugins;

public sealed class PluginButler : IPluginInstaller, IPluginUninstaller
{
    #region Fields

    /// <summary>
    /// Represents list of candidate for deletion.
    /// </summary>
    /// <remarks>
    /// When a plugin is asked for deletion, it's only removed after a restart
    /// of Lanceur. If user ask deletion of a plugin and comes later, we need
    /// to know what plugins are asked for deletion to not display them again
    /// and again.
    /// </remarks>
    private static readonly Dictionary<string, IPluginManifest> MutableUninstallationCandidates = new();

    private readonly IAppLogger _log;
    private readonly IPluginValidationRule<PluginValidationResult, PluginManifest> _pluginValidationRule;

    #endregion Fields

    #region Constructors

    public PluginButler(
            IAppLoggerFactory appLoggerFactory,
            IPluginValidationRule<PluginValidationResult, PluginManifest> pluginValidationRule)
    {
        ArgumentNullException.ThrowIfNull(appLoggerFactory, nameof(appLoggerFactory));
        ArgumentNullException.ThrowIfNull(pluginValidationRule, nameof(pluginValidationRule));

        _pluginValidationRule = pluginValidationRule;
        _log = appLoggerFactory.GetLogger<PluginButler>();
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Represents the plugins the user asked to delete
    /// </summary>
    public IEnumerable<IPluginManifest> UninstallationCandidates => MutableUninstallationCandidates.Values;

    #endregion Properties

    #region Methods

    private static void AddCandidate(IPluginManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var dir = Path.GetDirectoryName(manifest.Dll);
        if (dir is null)
            throw new NullReferenceException($"No directory defined in the manifest for plugin '{manifest.Name}'");

        MutableUninstallationCandidates.TryAdd(dir, manifest);
    }

    private static void RemoveCandidate(IPluginManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var dir = Path.GetDirectoryName(manifest.Dll);
        if (dir is null)
            throw new NullReferenceException($"No directory defined in the manifest for plugin '{manifest.Name}'");

        MutableUninstallationCandidates.Remove(dir);
    }

    private async Task<IOperationScheduler> GetOperationSchedulerAsync() =>
                await OperationSchedulerFactory.RetrieveFromFileAsync(Locations.MaintenanceLogBookPath);

    public async Task ExecutePlanAsync()
    {
        var os = await GetOperationSchedulerAsync();
        await os.ExecutePlanAsync();
    }

    public async Task<bool> HasMaintenanceAsync()
    {
        var os = await GetOperationSchedulerAsync();
        return os.GetState().OperationCount > 0;
    }

    public async Task<PluginInstallationResult> SubscribeForInstallAsync(string packagePath)
    {
        using var zip = ZipFile.OpenRead(packagePath);
        var config = (from entry in zip.Entries
                      where entry.Name == Locations.ManifestFileName
                      select entry).SingleOrDefault();

        if (config == null)
        {
            _log.Warning("No plugin manifest in package '{packagePath}'", packagePath);
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

        // If it is an update, the plugin to be updated  is in use.
        // A restart is then needed. Put the info in the maintenance
        // log and display the Plugin in the list of plugins.
        // Restart will really install it.
        await SubscribeForUninstallAsync(manifest);

        var tempPath = FileHelper.GetRandomTempFile(".zip");
        File.Copy(packagePath, tempPath);

        var os = await GetOperationSchedulerAsync();
        await os.AddOperation(
                    OperationFactory.UnzipDirectory(tempPath, Locations.FromManifest(manifest).PluginDirectoryPath))
                .SavePlanAsync();

        return PluginInstallationResult.Success(manifest, true);
    }

    public async Task<string> SubscribeForInstallAsync()
    {
        var os = await GetOperationSchedulerAsync();
        await os.ExecutePlanAsync();
        return string.Empty;
    }

    public async Task<PluginInstallationResult> SubscribeForInstallFromWebAsync(string url)
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

        return await SubscribeForInstallAsync(path);
    }

    public async Task SubscribeForUninstallAsync(IPluginManifest manifest)
    {
        var os = await GetOperationSchedulerAsync();
        var dir = Path.GetDirectoryName(manifest.Dll);

        if (dir is null)
            throw new DirectoryNotFoundException($"Cannot find plugin directory for plugin '{manifest.Name}'.");

        _log.Info("Add '{dir}' to directory to remove.", dir);
        AddCandidate(manifest);

        os.AddOperation(OperationFactory.RemoveDirectory(Locations.GetAbsolutePath(dir)))
          .RemoveOperation(OperationInfo.UnZip("destination", Locations.GetAbsolutePath(dir)));

        await os.SavePlanAsync();
    }

    #endregion Methods
}