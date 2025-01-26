using System.Data.SQLite;
using System.Web.Bookmarks.Configuration;
using System.Web.Bookmarks.Domain;
using Dapper;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Repositories;

public class GeckoBrowserBookmarkRepository : IBookmarkRepository
{
    #region Fields

    private  readonly string _bookmarksPath;
    private readonly string _cacheKey;
    private readonly ILogger<GeckoBrowserBookmarkRepository> _logger;

    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public GeckoBrowserBookmarkRepository(IMemoryCache memoryCache, ILoggerFactory loggerFactory, IGeckoBrowserConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(loggerFactory);


        _logger = loggerFactory.CreateLogger<GeckoBrowserBookmarkRepository>();
        var query = new IniFileLoader(loggerFactory).LoadQuery(configuration.IniFilename.ExpandPath());
        var path = query.GetDefaultProfile();

        _bookmarksPath = configuration.Database.Format(path).ExpandPath();
        ConfiguredBrowser = configuration.CacheKey;
        _cacheKey = $"{configuration.CacheKey}_{nameof(ChromeBookmarksRepository)}";
        _memoryCache = memoryCache;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public string ConfiguredBrowser { get;  }

    #endregion

    #region Methods

    private IEnumerable<Bookmark> FetchAll()
    {
        if (_bookmarksPath.IsNullOrWhiteSpace()) return [];
        
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
                   _cacheKey,
                   IEnumerable<Bookmark> (_)
                       => new SQLiteConnection(_bookmarksPath.ToSQLiteConnectionString()).Query<Bookmark>(sql)
               ) ??
               Array.Empty<Bookmark>();
    }

    ///<inheritdoc />
    public IEnumerable<Bookmark> GetBookmarks() => FetchAll();

    ///<inheritdoc />
    public IEnumerable<Bookmark> GetBookmarks(string filter) => FetchAll().Where(b => b.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));

    ///<inheritdoc />
    public bool IsBookmarkSourceAvailable()
    {
        if (File.Exists(_bookmarksPath))
        {
            _logger.LogTrace("Determined bookmarks path for Gecko-based engine: {BookmarksPath}", _bookmarksPath);
            return true;
        }

        _logger.LogWarning("Bookmark Source is not available. [{BookmarksPath}]", _bookmarksPath);
        return false;
    }

    #endregion
}