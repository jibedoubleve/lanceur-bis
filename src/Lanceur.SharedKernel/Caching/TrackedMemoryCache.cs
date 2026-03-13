using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Caching;

/// <summary>
///     A decorator around <see cref="IMemoryCache" /> that logs every cache operation
///     (create, hit, miss, remove, dispose) using the injected <see cref="ILogger{TCategoryName}" />.
/// </summary>
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

    /// <summary>
    ///     Creates a new cache entry for the specified <paramref name="key" /> and logs the operation at Debug level.
    /// </summary>
    public ICacheEntry CreateEntry(object key)
    {
        _logger.LogTrace("Cache: Creating cache with key {Key}", key);
        return _memoryCache.CreateEntry(key);
    }

    /// <summary>
    ///     Disposes the underlying cache and logs the operation at Trace level.
    /// </summary>
    public void Dispose()
    {
        _logger.LogTrace($"Cache: Disposing {nameof(TrackedMemoryCache)}");
        _memoryCache.Dispose();
    }

    /// <summary>
    ///     Removes the cache entry associated with the specified <paramref name="key" /> and logs the operation at Debug level.
    /// </summary>
    public void Remove(object key)
    {
        _logger.LogTrace("Cache: Removing cache with key {Key}", key);
        _memoryCache.Remove(key);
    }

    /// <summary>
    ///     Attempts to retrieve the value for the specified <paramref name="key" />.
    ///     Logs a Trace message on cache hit and a Debug message on cache miss.
    /// </summary>
    public bool TryGetValue(object key, out object? value)
    {
        var hasHit = _memoryCache.TryGetValue(key, out value);
        const string template = "Cache: {Hit} cache with key {Key}";

        if (hasHit) { _logger.LogTrace(template, "Hit", key); }
        else { _logger.LogTrace(template, "Miss", key); }

        return hasHit;
    }

    #endregion
}