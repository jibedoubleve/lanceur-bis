using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins
{
    public class PluginUninstaller : IPluginUninstaller
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

        private static async Task Save(IEnumerable<UninstallCandidate> candidates)
        {
            if (!candidates.Any() && File.Exists(PluginLocation.UninstallManifest))
            {
                File.Delete(PluginLocation.UninstallManifest);
                return;
            }

            var json = JsonConvert.SerializeObject(candidates);
            await File.WriteAllTextAsync(PluginLocation.UninstallManifest, json);
        }

        public async Task<IEnumerable<UninstallCandidate>> GetCandidatesAsync()
        {
            if (!File.Exists(PluginLocation.UninstallManifest)) { return Array.Empty<UninstallCandidate>(); }

            var json = await File.ReadAllTextAsync(PluginLocation.UninstallManifest);
            return JsonConvert.DeserializeObject<IEnumerable<UninstallCandidate>>(json);
        }

        public bool HasCandidateForUninstall() => File.Exists(PluginLocation.UninstallManifest);

        public async Task SubscribeForUninstallAsync(IPluginManifest manifest)
        {
            var candidates = (await GetCandidatesAsync()).ToList();
            var alreadyCandidate = (from c in candidates
                                    where c.Directory == manifest.Dll.GetDirectoryName()
                                    select c).Any();

            if (alreadyCandidate) { return; }

            candidates.Add(new UninstallCandidate(manifest.Dll.GetDirectoryName()));

            await Save(candidates);
        }

        public async Task UninstallAsync()
        {
            var retryCandidates = new List<UninstallCandidate>();

            foreach (var candidate in await GetCandidatesAsync())
            {
                var directory = PluginLocation.FromDirectory(candidate.Directory);
                if (directory.Exists())
                {
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
            }
            await Save(retryCandidates);
        }

        #endregion Methods
    }
}