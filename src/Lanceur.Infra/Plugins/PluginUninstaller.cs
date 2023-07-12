using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;

namespace Lanceur.Infra.Plugins
{
    public class PluginUninstaller : IPluginUninstaller
    {
        #region Fields

        private static readonly string _file;

        #endregion Fields

        #region Constructors

        static PluginUninstaller()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Lanceur2");
            _file = Path.Combine(dir, ".plugin-uninstall");
        }

        #endregion Constructors

        #region Methods

        public async Task<IEnumerable<UninstallCandidate>> GetCandidatesAsync()
        {
            if (!File.Exists(_file)) { return Array.Empty<UninstallCandidate>(); }

            var json = await File.ReadAllTextAsync(_file);
            return JsonConvert.DeserializeObject<IEnumerable<UninstallCandidate>>(json);
        }

        private async Task Save(IEnumerable<UninstallCandidate> candidates)
        {
            var json = JsonConvert.SerializeObject(candidates);
            await File.WriteAllTextAsync(_file, json);
        }

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

        public Task UninstallAsync()
        {
            throw new NotImplementedException();
        }

        #endregion Methods

        #region Classes


        #endregion Classes
    }
}