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

    private IBookmarkRepository CreateBookmarkRepository(IBookmarkRepository repository)
    {
        if (!repository.IsBookmarkSourceAvailable()) throw new NotSupportedException($"Bookmark source is not available for {repository.ConfiguredBrowser}");

        return repository;
    }

    ///<inheritdoc />
    public IBookmarkRepository CreateBookmarkRepository(Browser browser)
    {
        return browser switch
        {
            Browser.Chrome  => CreateBookmarkRepository(new ChromeBookmarksRepository(_memoryCache)),
            Browser.Firefox => CreateBookmarkRepository(new FireFoxBookmarkRepository(_memoryCache, _loggerFactory)),
            Browser.Zen     => CreateBookmarkRepository(new ZenSqlBookmarkRepository(_memoryCache, _loggerFactory)),
            _               => throw new ArgumentOutOfRangeException(nameof(browser), browser, null)
        };
    }

    ///<inheritdoc />
    public IBookmarkRepository CreateBookmarkRepository(string browser)
    {
        var browserEnum = Enum.Parse<Browser>(browser, true);
        return CreateBookmarkRepository(browserEnum);
    }

    #endregion
}