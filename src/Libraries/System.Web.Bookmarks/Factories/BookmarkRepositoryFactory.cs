using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Repositories;
using System.Web.Bookmarks.RepositoryConfiguration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Factories;

public class BookmarkRepositoryFactory : IBookmarkRepositoryFactory
{
    #region Fields

    private readonly ILoggerFactory _loggerFactory;

    private readonly IMemoryCache _memoryCache;

    private Browser? _previousBrowserCache;

    #endregion

    #region Constructors

    public BookmarkRepositoryFactory(IMemoryCache memoryCache, ILoggerFactory loggerFactory)
    {
        _memoryCache = memoryCache;
        _loggerFactory = loggerFactory;
    }

    #endregion

    #region Methods

    private IBookmarkRepository BuildBookmarkRepository(Browser browser)
    {
        _previousBrowserCache ??= browser;
        IBookmarkRepository repository = browser switch
        {
            Browser.Chrome  => new ChromiumBrowserBookmarks(_memoryCache, BrowserConfigurationFactory.Chrome),
            Browser.Edge    => new ChromiumBrowserBookmarks(_memoryCache, BrowserConfigurationFactory.Edge),
            Browser.Firefox => new GeckoBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Firefox),
            Browser.Zen     => new GeckoBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Zen),
            _               => throw new ArgumentOutOfRangeException(nameof(browser), browser, null)
        };

        if (_previousBrowserCache == browser) return repository;

        // Invalidate bookmarks cache when browser changed
        _memoryCache.Remove(_previousBrowserCache);
        _previousBrowserCache = browser;

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