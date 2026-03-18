using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Macros;

[Macro("multi")]
[Description("Allow to start multiple alias at once")]
public sealed class MultiMacro : MacroQueryResult
{
    #region Fields

    private readonly int _delay;
    private readonly IExecutionService _executionService;
    private readonly ILogger<MultiMacro> _logger;
    private readonly Lazy<ISearchService> _searchService;

    private static readonly Conditional<int> DefaultDelay = new(0, 1_000);

    #endregion

    #region Constructors

    public MultiMacro(
        IExecutionService executionService,
        Lazy<ISearchService> searchService,
        ILogger<MultiMacro> logger)
    {
        _executionService = executionService;
        _searchService = searchService;
        _logger = logger;
        _delay = DefaultDelay;
    }

    #endregion

    #region Methods

    private async Task<AliasQueryResult?> GetAlias(Cmdline cmdline)
    {
        List<QueryResult> t = [];
        await _searchService.Value.SearchAsync(t, cmdline);
        var macro = t.FirstOrDefault();

        if (macro is null)
        {
            _logger.LogInformation("Macro '{CmdlineName}' not found.", cmdline.Name);
            return null;
        }

        return macro as AliasQueryResult;
    }

    public override SelfExecutableQueryResult Clone() => new MultiMacro(_executionService, _searchService, _logger);

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        var items = Parameters?.Split('@') ?? [];
        var aliases = new List<AliasQueryResult>();

        foreach (var item in items)
        {
            var toAdd = await GetAlias(new Cmdline(item));
            if (toAdd is not null) { aliases.Add(toAdd); }
        }

        _ = _executionService.ExecuteMultiple(aliases, _delay);

        return await NoResultAsync;
    }

    #endregion
}