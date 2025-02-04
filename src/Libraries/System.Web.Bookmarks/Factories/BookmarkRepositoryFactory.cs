using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Repositories;
using System.Web.Bookmarks.RepositoryConfiiguration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Factories;

public class BookmarkRepositoryFactory : IBookmarkRepositoryFactory
{
    #region Fields

    private readonly ILoggerFactory _loggerFactory;

    private readonly IMemoryCache _memoryCache;

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
        return browser switch
        {
            Browser.Chrome  => new ChromiumBrowserBookmarks(_memoryCache, BrowserConfigurationFactory.Chrome),
            Browser.Edge    => new ChromiumBrowserBookmarks(_memoryCache, BrowserConfigurationFactory.Edge),
            Browser.Firefox => new GeckoBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Firefox),
            Browser.Zen     => new GeckoBrowserBookmarks(_memoryCache, _loggerFactory, BrowserConfigurationFactory.Zen),
            _               => throw new ArgumentOutOfRangeException(nameof(browser), browser, null)
        };
    }

    ///<inheritdoc />
    public IBookmarkRepository BuildBookmarkRepository(string browser)
    {
        var browserEnum = Enum.Parse<Browser>(browser, true);
        return BuildBookmarkRepository(browserEnum);
    }

    #endregion
}