using System.Text.Json.Nodes;
using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Factories;
using System.Web.Bookmarks.RepositoryConfiguration;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Repositories;

public class BlinkBrowserBookmarks : IBookmarkRepository
{
    #region Fields

    private readonly ILogger<BlinkBrowserBookmarks> _logger;
    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public BlinkBrowserBookmarks(
        IMemoryCache memoryCache,
        ILoggerFactory loggerFactory,
        IBlinkBrowserConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(configuration);

        _logger = loggerFactory.GetLogger<BlinkBrowserBookmarks>();
        _memoryCache = memoryCache;

        Path = configuration.Path;
        CacheKey = configuration.CacheKey;

        _logger.LogTrace("Using {Browser} based browser bookmarks path is {Path}", "Chrome", Path);
    }

    #endregion

    #region Properties

    private string Path { get; }

    /// <inheritdoc />
    public string CacheKey { get; }

    #endregion

    #region Methods

    private static void FetchAll(JsonNode? jsonNode, List<Bookmark> results)
    {
        while (true)
        {
            switch (jsonNode)
            {
                case null: return;
                case JsonArray array:
                {
                    foreach (var item in array) FetchAll(item, results);
                    return;
                }
                case JsonObject jsonObject when jsonObject.ContainsKey("children"):
                    jsonNode = jsonObject["children"];
                    continue;
                case JsonObject jsonObject:
                {
                    var name = jsonObject["name"]?.ToString();
                    var url = jsonObject["url"]?.ToString();
                    var order = jsonObject["date_last_used"]?.ToString() ?? "0";

                    if (name is not null && url is not null)
                        results.Add(new() { Name = name, Url = url, SortKey = order });
                    break;
                }
            }

            break;
        }
    }

    private IEnumerable<Bookmark> FetchAll(string? filter = null)
    {
        var bookmarks = _memoryCache.GetOrCreate(
            CacheKey,
            IEnumerable<Bookmark> (_) =>
            {
                var json = GetJson();
                var node = JsonNode.Parse(json);
                var results = new List<Bookmark>();

                var startNode = node?["roots"]?["bookmark_bar"];
                if (startNode is not null) FetchAll(startNode, results);
                return results.OrderByDescending(e => e.SortKey);
            },
            CacheEntryOptions.Default
        )!;

        _logger.LogTrace(
            "(Chromium) Getting bookmarks with filter '{Filter}' in path {Path}",
            filter ?? "<empty>",
            Path
        );
        return filter is null
            ? bookmarks
            : bookmarks.Where(e => e.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));
    }

    private string GetJson()
    {
        if (!File.Exists(Path))
        {
            _logger.LogWarning("(Chromium) Cannot find bookmark at {Path}", Path);
            return  "{}";
        }

        return File.ReadAllText(Path);
    }

    /// <inheritdoc />
    public IEnumerable<Bookmark> GetBookmarks() => FetchAll();

    /// <inheritdoc />
    public IEnumerable<Bookmark> GetBookmarks(string filter) => FetchAll(filter);

    public bool IsBookmarkSourceAvailable() => File.Exists(Path);

    #endregion
}