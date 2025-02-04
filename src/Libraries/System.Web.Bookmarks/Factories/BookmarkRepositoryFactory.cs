using System.Web.Bookmarks.Configuration;
using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Repositories;
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

    ///<inheritdoc />
    public IBookmarkRepository BuildBookmarkRepository(Browser browser)
    {
        return browser switch
        {
            Browser.Chrome  => new ChromeBookmarksRepository(_memoryCache),
            Browser.Edge    => new EdgeBookmarksRepository(_memoryCache),
            Browser.Firefox => new GeckoBrowserBookmarkRepository(_memoryCache, _loggerFactory, BrowserConfiguration.Firefox),
            Browser.Zen     => new GeckoBrowserBookmarkRepository(_memoryCache, _loggerFactory, BrowserConfiguration.Zen),
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