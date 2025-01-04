using Humanizer;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Utils;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class AliasStore : IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;

    private readonly IMemoryCache _cache;
    private readonly ILogger<AliasStore> _logger;

    #endregion

    #region Constructors

    public AliasStore(IServiceProvider serviceProvider)
    {
        _aliasRepository = serviceProvider.GetService<IAliasRepository>();
        _logger = serviceProvider.GetService<ILoggerFactory>()
                                 .GetLogger<AliasStore>();
        _cache = serviceProvider.GetService<IMemoryCache>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public StoreOrchestration StoreOrchestration => StoreOrchestration.SharedAlwaysActive();

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll() => _aliasRepository.GetAll();

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        using var _ = _logger.MeasureExecutionTime(this);

        if (_cache.TryGetValue(query.GetCacheKey(), out IEnumerable<QueryResult> cacheEntry))
        {
            _logger.LogTrace("The query {Query} is in the cache.", query.Name);
            return cacheEntry;
        }

        _logger.LogTrace("The query {Query} is NOT in the cache.", query.Name);
        var entry = _aliasRepository.Search(query.Name).ToArray();
        _cache.Set(query.GetCacheKey(), entry, 15.Seconds());
        return entry;
    }

    #endregion
}