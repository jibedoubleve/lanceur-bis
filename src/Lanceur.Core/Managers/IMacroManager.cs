using Lanceur.Core.Models;

namespace Lanceur.Core.Managers;

public interface IMacroManager
{
    #region Methods

    /// <summary>
    /// Get the list of all the macro you can have
    /// </summary>
    /// <returns>The list of macros</returns>
    IEnumerable<string> GetAll();

    /// <summary>
    /// Go throught the collection and update any macro with
    /// data of the search query.
    /// </summary>
    /// <param name="collection">The collection to parse</param>
    /// <returns>
    /// The updated collection. It doesn't touch non macro <see cref="QueryResult"/>
    /// </returns>
    /// <remarks>
    /// This method <b>do not</b> update the database.
    /// This <b>doesn't</b> touch <see cref="QueryResult"/> that
    /// are <b>not</b> macro </remarks>
    IEnumerable<QueryResult> Handle(QueryResult[] collection);

    /// <summary>
    /// Update the macro with the information of the user query.
    /// It'll fill the <see cref="ExecutableQueryResult.Parameters"/> with
    /// the parameters from the query the user entered. It'll remove any '@'
    /// from <see cref="QueryResult.Name"/>
    /// </summary>
    /// <param name="collection"><see cref="QueryResult"/> to handle</param>
    /// <returns>
    /// The updated macro.
    /// </returns>
    /// <remarks>
    /// This method <b>do not</b> update the database.
    /// This <b>doesn't</b> touch <see cref="QueryResult"/> that
    /// are <b>not</b> macro
    /// </remarks>
    QueryResult Handle(QueryResult item);

    #endregion Methods
}