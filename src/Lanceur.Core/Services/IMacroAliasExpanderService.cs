using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
///     Resolves macro aliases into executable macro instances.
/// </summary>
/// <remarks>
///     A macro alias is a user-defined shortcut (stored in the database) that points to a macro
///     identified by its <c>@NAME@</c> pattern. This service matches that alias against the registered
///     <see cref="MacroQueryResult" /> singletons, clones the matching template, and applies the
///     alias's own properties (name, parameters, description, etc.) onto the clone.
///     <para>
///         Macros are registered at startup via <c>AddMacroServices()</c>, which discovers all classes
///         decorated with <see cref="Lanceur.Core.MacroAttribute" /> and injects them as
///         <see cref="MacroQueryResult" /> singletons.
///     </para>
/// </remarks>
public interface IMacroAliasExpanderService
{
    #region Methods

    /// <summary>
    ///     Expands macro aliases within a collection of <see cref="QueryResult" /> objects.
    ///     In other words, clone the macro template and update it with the information provided in the command line (query)
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
    IEnumerable<QueryResult> Expand(params QueryResult[] collection);

    #endregion
}