namespace Lanceur.Core.Plugins
{
    public interface IPluginInstaller
    {
        #region Methods

        IPluginManifest Install(string packagePath);

        Task<IPluginManifest> InstallFromWebAsync(string url);

        #endregion Methods
    }
}