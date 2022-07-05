using Lanceur.Core.Models;

namespace Lanceur.Core.Managers
{
    public interface IMacroManager
    {
        #region Methods

        /// <summary>
        /// Get the list of all the macro you can have
        /// </summary>
        /// <returns>The list of macros</returns>
        IEnumerable<string> GetAll();

        /// <summary>
        /// Go throught he collection and replace the macro with
        /// executable behaviour.
        /// </summary>
        /// <param name="collection">The collection to parse</param>
        /// <returns>
        /// The collection with the macro behaviour. It Doesn't
        /// touch non macro <see cref="QueryResult"/>
        /// </returns>
        IEnumerable<QueryResult> Handle(IEnumerable<QueryResult> collection);

        /// <summary>
        /// Replace the macro with executable behaviour.
        /// </summary>
        /// <param name="collection"><see cref="QueryResult"/> to handle</param>
        /// <returns>
        /// The macro with the behaviour. It Doesn't touch non macro <see cref="QueryResult"/>
        /// </returns>
        QueryResult Handle(QueryResult item);

        #endregion Methods
    }
}