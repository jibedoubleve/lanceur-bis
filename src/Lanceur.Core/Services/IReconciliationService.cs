using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IReconciliationService
{
    #region Methods

    /// <summary>
    ///     Asynchronously attempts to infer and propose a description based on the file name
    ///     and any available information related to it. The inferred description is then
    ///     set to the description property.
    /// </summary>
    /// <param name="aliases">
    ///     A collection of alias query results used to gather relevant information for proposing the
    ///     description.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProposeDescriptionAsync(IEnumerable<AliasQueryResult> aliases);

    /// <summary>
    ///     Asynchronously attempts to infer and propose a description based on the file name
    ///     and any available information related to it. The inferred description is then
    ///     set to the description property.
    /// </summary>
    /// <param name="alias">A single alias query result used to gather relevant information for proposing the description.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProposeDescriptionAsync(AliasQueryResult alias);

    #endregion
}