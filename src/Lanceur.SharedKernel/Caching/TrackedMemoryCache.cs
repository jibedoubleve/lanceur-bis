using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Caching;

public class TrackedMemoryCache : IMemoryCache
{
    #region Fields

    private readonly ILogger<TrackedMemoryCache> _logger;
    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public TrackedMemoryCache(IMemoryCache memoryCache, ILogger<TrackedMemoryCache> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    #endregion

    #region Methods

    public ICacheEntry CreateEntry(object key)
    {
        _logger.LogTrace("Creating cache with key '{Key}'", key);
        return _memoryCache.CreateEntry(key);
    }

    public void Dispose()
    {
        _logger.LogTrace($"Disposing {nameof(TrackedMemoryCache)}");
        _memoryCache.Dispose();
    }

    public void Remove(object key)
    {
        _logger.LogTrace("Removing cache with key '{Key}'", key);
        _memoryCache.Remove(key);
    }

    public bool TryGetValue(object key, out object value)
    {
        var hasHit = _memoryCache.TryGetValue(key, out value);
        var hit = hasHit ? "Hit" : "Miss";

        _logger.LogTrace("{Hit} cache with key '{Key}'", hit, key);

        return hasHit;
    }

    #endregion
}