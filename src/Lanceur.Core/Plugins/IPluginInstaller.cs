namespace Lanceur.Core.Plugins
{
    public interface IPluginInstaller
    {
        #region Methods

        PluginInstallationResult Install(string packagePath);

        Task<PluginInstallationResult> InstallFromWebAsync(string url);

        #endregion Methods
    }
}