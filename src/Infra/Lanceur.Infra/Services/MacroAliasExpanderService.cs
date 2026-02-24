using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

/// <inheritdoc/>
public class MacroAliasExpanderService : Core.Services.IMacroAliasExpanderService
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
    public IEnumerable<QueryResult> Expand(params QueryResult[] collection)
    {
        using var _ = _logger.WarnIfSlow(this);
        var result = collection.Select(Expand)
                               .Where(item => item is not null)
                               .ToArray();
        return result;
    }

    #endregion
}