using Lanceur.Core.Plugins;
using System.Reflection;

namespace Lanceur.Core.Plugins
{
    public interface IPluginManager
    {
        #region Methods

        IEnumerable<IPlugin> CreatePlugin(Assembly assembly);

        Assembly LoadPluginAsm(string path);

        #endregion Methods
    }
}