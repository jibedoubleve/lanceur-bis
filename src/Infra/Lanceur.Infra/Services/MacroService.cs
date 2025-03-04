using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class MacroService : MacroCachedService, IMacroService
{
    #region Constructors

    public MacroService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    #endregion

    #region Properties

    public int MacroCount => MacroTemplates?.Count ?? 0;

    #endregion

    #region Methods

    /// <summary>
    /// Expands a macro alias into its corresponding macro definition by cloning the related template
    /// and applying the alias's properties.
    /// </summary>
    /// <param name="item">The <see cref="QueryResult"/> to process.</param>
    /// <returns>
    /// The expanded <see cref="MacroQueryResult"/> if the input is a valid macro alias.
    /// Returns the original <see cref="QueryResult"/> if it's not an alias.
    /// Returns <c>null</c> if the alias refers to a misconfigured macro or if the resolved instance is of an unexpected type.
    /// </returns>
    internal QueryResult ExpandMacroAlias(QueryResult item)
    {
        if (item is not AliasQueryResult alias || !alias.IsMacro()) return item;

        if (!MacroTemplates.TryGetValue(alias.GetMacroName(), out var instance))
        {
            /* Well, this is a misconfigured macro, log it and forget it */
            Logger.LogWarning(
                "User has misconfigured a Macro with name {FileName}. Fix the name of the macro or remove the alias from the database",
                alias.FileName
            );
            return null;
        }

        if (instance is not MacroQueryResult i)
        {
            Logger.LogWarning("Cannot cast '{Instance}' into '{MacroQueryResult}'", instance.GetType(), typeof(MacroQueryResult));
            return null;
        }

        var macro = i.Clone();
        macro.Name = alias.Name;
        macro.Parameters = alias.Parameters;
        macro.Description = alias.Description;
        return macro;
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> ExpandMacroAlias(QueryResult[] collection)
    {
        using var _ = Logger.WarnIfSlow(this);
        var result = collection.Select(ExpandMacroAlias)
                               .Where(item => item is not null)
                               .ToArray();
        return result;
    }

    #endregion
}