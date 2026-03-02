using System.Data.SQLite;
using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Factories;
using System.Web.Bookmarks.RepositoryConfiguration;
using Dapper;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Repositories;

public class GeckoBrowserBookmarks : IBookmarkRepository
{
    #region Fields

    private  readonly string _bookmarksPath;
    private readonly ILogger<GeckoBrowserBookmarks> _logger;

    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public GeckoBrowserBookmarks(
        IMemoryCache memoryCache,
        ILoggerFactory loggerFactory,
        IGeckoBrowserConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(loggerFactory);


        _logger = loggerFactory.CreateLogger<GeckoBrowserBookmarks>();
        var query = new IniFileLoader(loggerFactory).LoadQuery(configuration.IniFilename.ExpandPath());
        var path = query.GetDefaultProfile();

        _bookmarksPath = configuration.Database.Format(path).ExpandPath();
        CacheKey = configuration.CacheKey;
        _memoryCache = memoryCache;
        _logger.LogTrace("Using {Browser} based browser bookmarks path is {Path}", "Gecko", _bookmarksPath);
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public string CacheKey { get;  }

    #endregion

    #region Methods

    private IEnumerable<Bookmark> FetchAll()
    {
        if (_bookmarksPath.IsNullOrWhiteSpace()) { return []; }

        const string sql = """
                           select 
                           	b.title as name,
                           	b.title as SortKey,
                           	p.url   as url
                           from 
                           	moz_bookmarks b
                           	inner join moz_places p on b.fk = p.id
                           where type = 1;
                           """;
        return _memoryCache.GetOrCreate(
                   CacheKey,
                   IEnumerable<Bookmark> (_)
                       => new SQLiteConnection(_bookmarksPath.ToSQLiteConnectionString()).Query<Bookmark>(sql),
                   CacheEntryOptions.Default
               ) ??
               Array.Empty<Bookmark>();
    }

    ///<inheritdoc />
    public IEnumerable<Bookmark> GetBookmarks() => FetchAll();

    ///<inheritdoc />
    public IEnumerable<Bookmark> GetBookmarks(string filter)
        => FetchAll().Where(b => b.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));

    /// <inheritdoc />
    public void InvalidateCache() => _memoryCache.Remove(CacheKey);

    ///<inheritdoc />
    public bool IsBookmarkSourceAvailable()
    {
        if (File.Exists(_bookmarksPath))
        {
            _logger.LogDebug("Determined bookmarks path for Gecko-based engine: {BookmarksPath}", _bookmarksPath);
            return true;
        }

        _logger.LogWarning("Bookmark Source is not available. [{BookmarksPath}]", _bookmarksPath);
        return false;
    }

    #endregion
}