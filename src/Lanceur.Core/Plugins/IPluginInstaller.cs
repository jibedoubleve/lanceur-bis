namespace Lanceur.Core.Plugins
{
    public interface IPluginInstaller
    {
        #region Methods
        
        /// <summary>
        /// Check whether the maintenance log book contains plugin to install
        /// </summary>
        /// <returns><c>True</c> if there are plugins to install; otherwise <c>False</c></returns>
        Task<bool> HasMaintenanceAsync();

        Task<PluginInstallationResult> InstallAsync(string packagePath);

        Task<string> InstallAsync();

        Task<PluginInstallationResult> InstallFromWebAsync(string url);

        #endregion Methods

    }
}