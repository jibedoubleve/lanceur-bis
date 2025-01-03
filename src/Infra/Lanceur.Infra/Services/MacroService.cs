using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Managers;

public class MacroService : MacroCachedService, IMacroService
{
    #region Constructors

    public MacroService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    #endregion

    #region Properties

    public int MacroCount => MacroInstances?.Count ?? 0;

    #endregion

    #region Methods

    /// <inheritdoc/>
    public IEnumerable<string> GetAll() => MacroInstances.Keys;

    /// <inheritdoc/>
    public IEnumerable<QueryResult> Handle(QueryResult[] collection)
    {
        using var _ = Logger.MeasureExecutionTime(this);
        var result = collection.Select(Handle)
                               .Where(item => item is not null)
                               .ToArray();
        return result;
    }

    /// <inheritdoc/>
    public QueryResult Handle(QueryResult item)
    {
        if (item is not AliasQueryResult alias || !alias.IsMacro()) return item;

        var macroInstances = MacroInstances;
        var macroName = alias.GetMacroName();

        if (!macroInstances.TryGetValue(macroName, out var instance))
        {
            /* Well, this is a misconfigured macro, log it and forget it */
            Logger.LogInformation(
                "User has misconfigured a Macro with name {FileName}. Fix the name of the macro or remove the alias from the database",
                alias.FileName
            );
            return null;
        }

        if (instance is not MacroQueryResult i) throw new NotSupportedException($"Cannot cast '{instance.GetType()}' into '{typeof(MacroQueryResult)}'");

        var macro = i.Clone();
        macro.Name = alias.Name;
        macro.Parameters = alias.Parameters;
        return macro;
    }

    #endregion
}