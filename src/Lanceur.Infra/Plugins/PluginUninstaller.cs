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

        private static readonly string _uninstallManifest = Environment.ExpandEnvironmentVariables(@"%appdata%\probel\lanceur2\.plugin-uninstall");
        private static readonly string _pluginRootDir;
        private readonly IAppLogger _log;

        #endregion Fields

        #region Constructors

        static PluginUninstaller()
        {
            _pluginRootDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Lanceur2");
        }

        public PluginUninstaller(IAppLoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            _log = loggerFactory.GetLogger<PluginUninstaller>();
        }

        #endregion Constructors

        #region Methods

        private static async Task Save(IEnumerable<UninstallCandidate> candidates)
        {
            if (!candidates.Any() && File.Exists(_uninstallManifest))
            {
                File.Delete(_uninstallManifest);
                return;
            }

            var json = JsonConvert.SerializeObject(candidates);
            await File.WriteAllTextAsync(_uninstallManifest, json);
        }

        public async Task<IEnumerable<UninstallCandidate>> GetCandidatesAsync()
        {
            if (!File.Exists(_uninstallManifest)) { return Array.Empty<UninstallCandidate>(); }

            var json = await File.ReadAllTextAsync(_uninstallManifest);
            return JsonConvert.DeserializeObject<IEnumerable<UninstallCandidate>>(json);
        }

        public bool HasCandidateForUninstall() => File.Exists(_uninstallManifest);

        public async Task SubscribeForUninstallAsync(IPluginConfiguration pluginConfiguration)
        {
            var candidates = (await GetCandidatesAsync()).ToList();
            var alreadyCandidate = (from c in candidates
                                    where c.Directory == pluginConfiguration.Dll.GetDirectoryName()
                                    select c).Any();

            if (alreadyCandidate) { return; }

            candidates.Add(new UninstallCandidate(pluginConfiguration.Dll.GetDirectoryName()));

            await Save(candidates);
        }

        public async Task UninstallAsync()
        {
            var retryCandidates = new List<UninstallCandidate>();

            foreach (var candidate in await GetCandidatesAsync())
            {
                var directory = new PluginDirectory(candidate.Directory);
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