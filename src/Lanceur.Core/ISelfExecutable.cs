using Lanceur.Core.Models;

namespace Lanceur.Core;

public interface ISelfExecutable : IElevated
{
    #region Methods

    /// <summary>
    ///     Executes the alias's built-in behaviour asynchronously.
    ///     The optional <paramref name="cmdline"/> can be used to parameterise the execution
    ///     when the implementation supports it.
    /// </summary>
    /// <param name="cmdline">
    ///     The command-line input used to configure the execution. Can be <see langword="null"/>
    ///     when no additional parameters are required.
    /// </param>
    /// <returns>
    ///     A collection of <see cref="QueryResult"/> items produced by the execution,
    ///     or a single result describing the outcome if no items are returned.
    /// </returns>
    Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null);

    #endregion
}