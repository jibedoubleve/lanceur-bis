using FileOperationScheduler.Infrastructure.Operations;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Plugins
{
    public class PluginUninstaller : PluginButler, IPluginUninstaller
    {
        #region Fields

        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public PluginUninstaller(IAppLoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            _log = loggerFactory.GetLogger<PluginUninstaller>();
        }

        #endregion Constructors

        #region Methods

        public async Task<bool> HasMaintenanceAsync()
        {
            var os = await GetOperationSchedulerAsync();
            return os.GetState().OperationCount > 0;
        }

        public async Task SubscribeForUninstallAsync(IPluginManifest manifest)
        {
            var os = await GetOperationSchedulerAsync();
            var dir = Path.GetDirectoryName(manifest.Dll);

            if (dir is null)
                throw new DirectoryNotFoundException($"Cannot find plugin directory for plugin '{manifest.Name}'.");

            _log.Info($"Add '{dir}' to directory to remove.");
            AddCandidate(manifest);
            
            os.AddOperation(OperationFactory.RemoveDirectory(Locations.GetAbsolutePath(dir)));
            await os.SavePlanAsync();
        }

        public async Task UninstallAsync()
        {
            var os = await GetOperationSchedulerAsync();
            await os.ExecutePlanAsync();
        }

        #endregion Methods
    }
}