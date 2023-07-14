using Lanceur.Core.Models;

namespace Lanceur.Core.Plugins
{
    public interface IPluginInstaller
    {
        #region Methods

        IPluginConfiguration Install(string packagePath);

        #endregion Methods
    }
}