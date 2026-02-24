using System.ComponentModel;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class ReservedAliasStore : Store, IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    private readonly ILogger<ReservedAliasStore> _logger;
    private readonly IEnumerable<QueryResult> _reservedAliases;

    #endregion

    #region Constructors

    /// <summary>
    ///     Generate a new instance. Look into the Executing Assembly to find reserved aliases.
    /// </summary>
    /// <param name="orchestrationFactory">Service Provider used to inject dependencies</param>
    /// <param name="aliasRepository">Repository of all the aliases</param>
    /// <param name="logger">Used for logging</param>
    /// <param name="reservedAliases">List of reserved aliases (builtin aliases)</param>
    /// <exception cref="ArgumentNullException">If assembly source is null or no ReservedKeywordSource configured</exception>
    /// <remarks>
    ///     Each reserved alias should be decorated with <see cref="ReservedAliasAttribute" />
    /// </remarks>
    public ReservedAliasStore(
        IStoreOrchestrationFactory orchestrationFactory,
        IAliasRepository aliasRepository,
        ILogger<ReservedAliasStore> logger,
        IEnumerable<SelfExecutableQueryResult> reservedAliases
    ) : base(orchestrationFactory)
    {
        _aliasRepository = aliasRepository;
        _logger = logger;
        _reservedAliases = reservedAliases;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => false;

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.SharedAlwaysActive();

    #endregion

    #region Methods


    private static IEnumerable<QueryResult> RefreshCounters(
        List<QueryResult> result,
        Dictionary<string, (long Id, int Count)> counters
    )
    {
        var orderedResult = result.Select(alias =>
            {
                if (counters is null) return alias;

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