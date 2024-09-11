using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.DependencyInjection;
using Splat;

namespace Lanceur.Infra.Macros;

[Macro("multi"), Description("Allow to start multiple alias at once")]
public class MultiMacro : MacroQueryResult
{
    #region Fields

    private readonly int _delay;
    private readonly IExecutionManager _executionManager;
    private readonly ISearchService _searchService;
    private readonly IServiceProvider _serviceProvider;

    private static readonly Conditional<int> DefaultDelay = new(0, 1_000);

    #endregion

    #region Constructors

    public MultiMacro(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _executionManager = serviceProvider.GetService<IExecutionManager>();
        _searchService = serviceProvider.GetService<ISearchService>();
        _delay = DefaultDelay;
    }

    public MultiMacro(int? delay = null, IExecutionManager executionManager = null, ISearchService searchService = null)
    {
        _delay = delay ?? DefaultDelay;
        _executionManager = executionManager ?? Locator.Current.GetService<IExecutionManager>();
        _searchService = searchService ?? Locator.Current.GetService<ISearchService>();
    }

    #endregion

    #region Methods

    private async Task<AliasQueryResult> GetAlias(Cmdline cmdline)
    {
        var t  = await _searchService.SearchAsync(cmdline);
        var macro = t.FirstOrDefault();
        return macro as AliasQueryResult;
    }

    public override SelfExecutableQueryResult Clone() => new MultiMacro(_serviceProvider);

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        var items = Parameters?.Split('@') ?? Array.Empty<string>();
        var aliases = new List<AliasQueryResult>();

        foreach (var item in items)
        {
            var toAdd = await GetAlias(new(item));
            aliases.Add(toAdd);
        }

        _ = _executionManager.ExecuteMultiple(aliases, _delay);

        return await NoResultAsync;
    }

    #endregion
}