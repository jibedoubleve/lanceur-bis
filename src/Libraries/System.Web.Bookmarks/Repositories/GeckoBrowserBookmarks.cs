using System.Data.SQLite;
using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.RepositoryConfiiguration;
using Dapper;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Repositories;

public class GeckoBrowserBookmarks : IBookmarkRepository
{
    #region Fields

    private  readonly string _bookmarksPath;
    private readonly string _cacheKey;
    private readonly ILogger<GeckoBrowserBookmarks> _logger;

    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public GeckoBrowserBookmarks(IMemoryCache memoryCache, ILoggerFactory loggerFactory, IGeckoBrowserConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(loggerFactory);


        _logger = loggerFactory.CreateLogger<GeckoBrowserBookmarks>();
        var query = new IniFileLoader(loggerFactory).LoadQuery(configuration.IniFilename.ExpandPath());
        var path = query.GetDefaultProfile();

        _bookmarksPath = configuration.Database.Format(path).ExpandPath();
        _cacheKey = $"{configuration.CacheKey}_{nameof(ChromiumBrowserBookmarks)}";
        _memoryCache = memoryCache;
    }

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