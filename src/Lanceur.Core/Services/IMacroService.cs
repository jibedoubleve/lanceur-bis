using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IMacroService
{
    #region Methods

    /// <summary>
    ///     Expands macro aliases within a collection of <see cref="QueryResult" /> objects.
    /// In other words, clone the macro template and update it with the informations provided in the command line (query)
    /// </summary>
    /// <param name="collection">An array of <see cref="QueryResult" /> items to process.</param>
    /// <returns>
    ///     A collection of <see cref="QueryResult" /> where macro aliases have been expanded.
    ///     Null results are filtered out.
    /// </returns>
    /// <remarks>
    ///     This method <b>do not</b> update the database.
    ///     This <b>doesn't</b> touch <see cref="QueryResult" /> that
    ///     are <b>not</b> macro
    /// </remarks>
    IEnumerable<QueryResult> ExpandMacroAlias(QueryResult[] collection);

    #endregion
}