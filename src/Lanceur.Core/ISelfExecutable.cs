using Lanceur.Core.Models;

namespace Lanceur.Core;

public interface ISelfExecutable : IElevated
{
    #region Methods

    Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null);

    #endregion
}