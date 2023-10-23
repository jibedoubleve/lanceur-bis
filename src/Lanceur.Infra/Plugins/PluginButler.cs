using FileOperationScheduler.Core;
using FileOperationScheduler.Infrastructure;
using Lanceur.Core.Plugins;

namespace Lanceur.Infra.Plugins
{
    public abstract class PluginButler
    {
        #region Fields

        private IOperationScheduler _operationScheduler;

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

        #endregion Fields

        #region Properties

        /// <summary>
        /// Represents the plugins the user asked to delete
        /// </summary>
        public IEnumerable<IPluginManifest> UninstallationCandidates => MutableUninstallationCandidates.Values;

        #endregion Properties

        #region Methods

        protected async Task<IOperationScheduler> GetOperationSchedulerAsync() => _operationScheduler ??=
            await OperationSchedulerFactory.RetrieveFromFileAsync(Locations.MaintenanceLogBookPath);

        protected static void AddCandidate(IPluginManifest manifest)
        {
            ArgumentNullException.ThrowIfNull(manifest);
            
            var dir = Path.GetDirectoryName(manifest.Dll);
            if (dir is null) throw new NullReferenceException($"No directory defined in the manifest for plugin '{manifest.Name}'");
            
            MutableUninstallationCandidates.Add(dir, manifest);
        }

        protected static void RemoveCandidate(IPluginManifest manifest)
        {
            ArgumentNullException.ThrowIfNull(manifest);
            
            var dir = Path.GetDirectoryName(manifest.Dll);
            if (dir is null) throw new NullReferenceException($"No directory defined in the manifest for plugin '{manifest.Name}'");
            
            MutableUninstallationCandidates.Remove(dir);
        }
        #endregion Methods
    }
}