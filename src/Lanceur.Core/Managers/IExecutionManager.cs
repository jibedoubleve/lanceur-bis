using Lanceur.Core.Models;

namespace Lanceur.Core.Managers
{
    public interface IExecutionManager
    {
        #region Methods

        Task ExecuteAsync(AliasQueryResult query);

        #endregion Methods
    }
}