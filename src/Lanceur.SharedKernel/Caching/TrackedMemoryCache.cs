using Lanceur.SharedKernel.Logging;
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
        _logger.LogDebug("TrackedMemoryCache: Creating cache");
        return _memoryCache.CreateEntry(key);
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing TrackedMemoryCache");
        _memoryCache.Dispose();
    }

    public void Remove(object key)
    {
        _logger.LogDebug("Removing cache for {@Key}", key);
        _memoryCache.Remove(key);
    }

    public bool TryGetValue(object key, out object value)
    {
        var result =  _memoryCache.TryGetValue(key, out value);

        if (result)
            _logger.LogDebug("Hit cache for '{Key}'", key);
        else
            _logger.LogDebug("Miss cache for {Key}", key);

        return result;
    }

    #endregion
}