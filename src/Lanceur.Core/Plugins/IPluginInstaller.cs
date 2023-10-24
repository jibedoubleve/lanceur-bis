namespace Lanceur.Core.Plugins;

public interface IPluginInstaller
{
    #region Methods

    Task<bool> HasMaintenanceAsync();

    Task<PluginInstallationResult> SubscribeForInstallAsync(string packagePath);

    Task<string> SubscribeForInstallAsync();

    Task<PluginInstallationResult> SubscribeForInstallFromWebAsync(string url);

    #endregion Methods
}