using Lanceur.Core.Models;
using System.Reflection;

namespace Lanceur.Core.Managers
{
    public interface IPluginManager
    {
        #region Methods

        QueryResult Activate(Type plugin);

        bool Exists(string dll);

        IEnumerable<Type> GetPluginTypes(Assembly asm);

        IEnumerable<Type> GetPluginTypes(string dll);

        #endregion Methods
    }
}