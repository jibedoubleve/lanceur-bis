using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins
{
    public class PluginUninstaller : IPluginUninstaller
    {
        private readonly IMaintenanceLogBook _maintenanceLogBook;

        #region Fields

        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        public PluginUninstaller(IAppLoggerFactory loggerFactory, IMaintenanceLogBook maintenanceLogBook)
        {
            _maintenanceLogBook = maintenanceLogBook;
            ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            _log = loggerFactory.GetLogger<PluginUninstaller>();
        }

        #endregion Constructors

        #region Methods

        public async Task<IEnumerable<MaintenanceCandidate>> GetUninstallCandidatesAsync() =>
            await _maintenanceLogBook.GetUninstallCandidatesAsync();

        public async Task<bool> HasMaintenanceAsync() =>
            (await _maintenanceLogBook.GetUninstallCandidatesAsync()).Any();

        public async Task SubscribeForUninstallAsync(IPluginManifest manifest)
        {
            var candidates = (await GetUninstallCandidatesAsync()).ToList();
            var alreadyCandidate = (from c in candidates
                                    where c.Directory == manifest.Dll.GetDirectoryName()
                                    select c).Any();

            if (alreadyCandidate)
            {
                return;
            }

            candidates.Add(MaintenanceCandidate.UninstallCandidate(manifest.Dll.GetDirectoryName()));

            await _maintenanceLogBook.SaveAsync(candidates.ToArray());
        }

        public async Task UninstallAsync()
        {
            var retryCandidates = new List<MaintenanceCandidate>();

            foreach (var candidate in await _maintenanceLogBook.GetUninstallCandidatesAsync())
            {
                var directory = PluginLocation.FromDirectory(candidate.Directory);

                if (!directory.Exists()) continue;

                try
                {
                    _log.Info($"Removing plugin at '{candidate.Directory}'");
                    directory.Delete();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to uninstall plugin at '{candidate.Directory}'", ex);
                    retryCandidates.Add(candidate);
                }
            }

            await _maintenanceLogBook.SaveAsync(retryCandidates.ToArray());
        }

        #endregion Methods
    }
}