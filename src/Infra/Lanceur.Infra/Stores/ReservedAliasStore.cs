using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public sealed class ReservedAliasStore : StoreBase, IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    private readonly ILogger<ReservedAliasStore> _logger;
    private readonly IEnumerable<QueryResult> _reservedAliases;

    #endregion

    #region Constructors

    public ReservedAliasStore(
        IStoreOrchestrationFactory orchestrationFactory,
        IAliasRepository aliasRepository,
        ILogger<ReservedAliasStore> logger,
        IEnumerable<SelfExecutableQueryResult> reservedAliases,
        ISection<StoreSection> storeSettings
    ) : base(orchestrationFactory, storeSettings)
    {
        _aliasRepository = aliasRepository;
        _logger = logger;
        _reservedAliases = reservedAliases;
    }

    #endregion

    #region Properties

    /// <inheritdoc cref="IStoreService.IsOverridable"/>
    public override bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.SharedAlwaysActive();

    #endregion

    #region Methods

    private static IEnumerable<QueryResult> RefreshCounters(
        List<QueryResult> result,
        Dictionary<string, (long Id, int Count)> counters
    )
    {
        var orderedResult = result.Select(alias => {
                if (counters is null) { return alias; }

                var r = counters.Where(counter => counter.Key == alias.Name)
                                .Select(a => a.Value)
                                .FirstOrDefault();
                alias.Id = r.Id;
                alias.Count = r.Count;
                return alias;
            }
        );
        return orderedResult;
    }

    /// <inheritdoc cref="IStoreService.GetAll" />
    public override IEnumerable<QueryResult> GetAll() => _reservedAliases;

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        using var _ = _logger.WarnIfSlow(this);
        var result = GetAll()
                     .Where(k => k.Name.StartsWith(cmdline.Name, StringComparison.CurrentCultureIgnoreCase))
                     .ToList();

        var counters = _aliasRepository.GetHiddenCounters();
        return RefreshCounters(result, counters)
               .OrderByDescending(x => x.Count)
               .ThenBy(x => x.Name);
    }

    #endregion
}