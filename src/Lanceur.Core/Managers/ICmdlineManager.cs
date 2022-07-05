using Lanceur.Core.Models;

namespace Lanceur.Core.Managers
{
    public interface ICmdlineManager
    {
        #region Methods

        Cmdline BuildFromText(string commandline);

        Cmdline CloneWithNewParameters(string newParameters, Cmdline cmd);

        #endregion Methods
    }
}