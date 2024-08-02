namespace Lanceur.Core.Plugins;

public interface IPluginManager
{
    #region Methods

    IEnumerable<IPlugin> CreatePlugin(string assemblyPath);

    #endregion Methods
}