using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

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
public class MacroAliasExpanderService : IMacroService
{
    #region Fields

    private readonly IEnumerable<MacroQueryResult> _macros;

    #endregion

    #region Constructors

    public MacroAliasExpanderService(
        ILogger<MacroAliasExpanderService> logger,
        IEnumerable<MacroQueryResult> macros
    )
    {
        _logger = logger;
        _macros = macros;
    }

    #endregion

    #region Properties

    private readonly ILogger _logger;

    public int MacroCount => _macros.Count();

    #endregion

    #region Methods

    /// <summary>
    ///     Expands a macro alias into its corresponding macro definition by cloning the related template
    ///     and applying the alias's properties.
    /// </summary>
    /// <param name="item">The <see cref="QueryResult" /> to process.</param>
    /// <returns>
    ///     The expanded <see cref="MacroQueryResult" /> if the input is a valid macro alias.
    ///     Returns the original <see cref="QueryResult" /> if it's not an alias.
    ///     Returns <c>null</c> if the alias refers to a misconfigured macro or if the resolved instance is of an unexpected
    ///     type.
    /// </returns>
    private QueryResult Expand(QueryResult item)
    {
        if (item is not AliasQueryResult alias || !alias.IsMacro()) return item;
        
        var foundMacro = _macros.FirstOrDefault(m => alias.GetMacroName() == m.Name);
        if (foundMacro is null)
        {
            /* Well, this is a misconfigured macro, log it and forget it */
            _logger.LogWarning(
                "User has misconfigured a Macro with name {FileName}. Fix the name of the macro or remove the alias from the database",
                alias.FileName
            );
            return null;
        }

        var macro = foundMacro.Clone();
        macro.Name = alias.Name;
        macro.Parameters = alias.Parameters;
        macro.Description = alias.Description.IsNullOrWhiteSpace() ? foundMacro.Description : alias.Description;
        macro.Count = alias.Count;
        macro.Id = alias.Id;
        return macro;
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> ExpandMacroAlias(params QueryResult[] collection)
    {
        using var _ = _logger.WarnIfSlow(this);
        var result = collection.Select(Expand)
                               .Where(item => item is not null)
                               .ToArray();
        return result;
    }

    #endregion
}