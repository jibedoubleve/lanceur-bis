namespace Lanceur.Core.Plugins;

public interface IPluginInstaller
{
    #region Methods

    Task<bool> HasMaintenanceAsync();

    Task<PluginInstallationResult> InstallAsync(string packagePath);

    Task<string> InstallAsync();

    Task<PluginInstallationResult> InstallFromWebAsync(string url);

    #endregion Methods
}