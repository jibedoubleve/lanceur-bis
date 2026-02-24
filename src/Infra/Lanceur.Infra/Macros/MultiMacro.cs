using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Infra.Macros;

[Macro("multi")]
[Description("Allow to start multiple alias at once")]
public class MultiMacro : MacroQueryResult
{
    #region Fields

    private readonly int _delay;
    private readonly IExecutionService _executionService;
    private readonly Lazy<ISearchService> _searchService;

    private static readonly Conditional<int> DefaultDelay = new(0, 1_000);

    #endregion

    #region Constructors

    public MultiMacro(IExecutionService executionService, Lazy<ISearchService> searchService)
    {
        _executionService = executionService;
        _searchService = searchService;
        _delay = DefaultDelay;
    }

    #endregion

    #region Methods

    private async Task<AliasQueryResult> GetAlias(Cmdline cmdline)
    {
        var t  = await _searchService.Value.SearchAsync(cmdline);
        var macro = t.FirstOrDefault();
        return macro as AliasQueryResult;
    }

    public override SelfExecutableQueryResult Clone() => new MultiMacro(_executionService, _searchService);

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var items = Parameters?.Split('@') ?? [];
        var aliases = new List<AliasQueryResult>();

        foreach (var item in items)
        {
            var toAdd = await GetAlias(new(item));
            aliases.Add(toAdd);
        }

        _ = _executionService.ExecuteMultiple(aliases, _delay);

        return await NoResultAsync;
    }

    #endregion
}