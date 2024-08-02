namespace Lanceur.Core.Plugins;

public interface IPluginUninstaller
{
    #region Properties

    /// <summary>
    /// Represents the plugins the user asked to delete
    /// </summary>
    public IEnumerable<IPluginManifest> UninstallationCandidates { get; }

    #endregion Properties

    #region Methods

    Task<bool> HasMaintenanceAsync();

    Task SubscribeForUninstallAsync(IPluginManifest pluginManifest);

    #endregion Methods
}