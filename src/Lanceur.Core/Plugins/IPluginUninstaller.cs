namespace Lanceur.Core.Plugins
{
    public interface IPluginUninstaller
    {
        #region Methods

        Task<IEnumerable<MaintenanceCandidate>> GetUninstallCandidatesAsync();

        Task<bool> HasMaintenanceAsync();

        Task SubscribeForUninstallAsync(IPluginManifest pluginManifest);

        Task UninstallAsync();

        #endregion Methods
    }
}