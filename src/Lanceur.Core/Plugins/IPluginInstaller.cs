using Lanceur.Core.Models;

namespace Lanceur.Core.Plugins
{
    public interface IPluginInstaller
    {
        #region Methods

        IPluginManifest Install(string packagePath);

        #endregion Methods
    }
}