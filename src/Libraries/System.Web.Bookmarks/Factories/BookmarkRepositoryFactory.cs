using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Repositories;
using System.Web.Bookmarks.RepositoryConfiguration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Factories;

public class BookmarkRepositoryFactory : IBookmarkRepositoryFactory
{
    #region Fields

    private readonly ILogger<BookmarkRepositoryFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public BookmarkRepositoryFactory(IMemoryCache memoryCache, ILoggerFactory loggerFactory)
    {
        _memoryCache = memoryCache;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<BookmarkRepositoryFactory>();
    }

    #endregion

    #region Properties

    public string? PreviousBrowserCacheKey { get; private set; }

    #endregion

    #region Methods

    private IBookmarkRepository BuildBookmarkRepository(Browser browser)
    {
        _logger.LogDebug("Returning bookmark repository for {Browser}", browser);
        IBookmarkRepository repository = browser switch
        {
            Browser.Chrome  => new BlinkBrowserBookmarks(_memoryCache,  _loggerFactory, BrowserConfigurationFactory.Chrome),
            Browser.Edge    => new BlinkBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Edge),
            Browser.Firefox => new GeckoBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Firefox),
            Browser.Zen     => new GeckoBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Zen),
            _               => throw new ArgumentOutOfRangeException(nameof(browser), browser, null)
        };

        PreviousBrowserCacheKey ??= repository.CacheKey;
        if (PreviousBrowserCacheKey is not null 
            && PreviousBrowserCacheKey != repository.CacheKey)
        {
            // Invalidate bookmarks cache when browser changed
            _memoryCache.Remove(PreviousBrowserCacheKey!);
            PreviousBrowserCacheKey = repository.CacheKey;
        }

        return repository;
    }

    ///<inheritdoc />
    public IBookmarkRepository BuildBookmarkRepository(string browser)
    {
        var browserEnum = Enum.Parse<Browser>(browser, true);
        return BuildBookmarkRepository(browserEnum);
    }

    #endregion
}