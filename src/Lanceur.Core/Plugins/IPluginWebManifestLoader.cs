namespace Lanceur.Core.Plugins;

public interface IPluginWebManifestLoader
{
    #region Methods

    Task<IEnumerable<IPluginWebManifest>> LoadFromWebAsync();

    #endregion Methods
}