using System.Data.SQLite;
using System.Web.Bookmarks.Domain;
using Dapper;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Repositories;

public abstract class SqlBookmarkRepository : IBookmarkRepository
{
    #region Fields

    private  readonly string _bookmarksPath;

    private readonly string _cacheKey;
    private readonly ILogger<SqlBookmarkRepository> _logger;

    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    protected SqlBookmarkRepository(IMemoryCache memoryCache, ILoggerFactory loggerFactory, (string Database, string IniFilename) configuration, string cacheKeyPrefix)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(cacheKeyPrefix);
        ArgumentNullException.ThrowIfNull(loggerFactory);


        _logger = loggerFactory.CreateLogger<SqlBookmarkRepository>();
        var query = new IniFileLoader(loggerFactory).LoadQuery(configuration.IniFilename.ExpandPath());
        var path = query.GetDefaultProfile();

        _bookmarksPath = configuration.Database.Format(path).ExpandPath();
        ConfiguredBrowser = cacheKeyPrefix;
        _cacheKey = $"{cacheKeyPrefix}_{nameof(ChromeBookmarksRepository)}";
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