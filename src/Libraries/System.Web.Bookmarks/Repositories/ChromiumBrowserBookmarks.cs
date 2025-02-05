using System.Text.Json.Nodes;
using System.Web.Bookmarks.Domain;
using System.Web.Bookmarks.Factories;
using System.Web.Bookmarks.RepositoryConfiguration;
using Microsoft.Extensions.Caching.Memory;

namespace System.Web.Bookmarks.Repositories;

public class ChromiumBrowserBookmarks : IBookmarkRepository
{
    #region Fields

    private readonly IMemoryCache _memoryCache;

    #endregion

    #region Constructors

    public ChromiumBrowserBookmarks(IMemoryCache memoryCache, IChromiumBrowserConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        _memoryCache = memoryCache;
        Path = configuration.Path;
        CacheKey = configuration.CacheKey;
    }

    #endregion

    #region Properties

    private string CacheKey { get; }

    private string Path { get; }

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

                    if (name is not null && url is not null) results.Add(new() { Name = name, Url = url, SortKey = order });
                    break;
                }
            }

            break;
        }
    }

    private IEnumerable<Bookmark>  FetchAll()
    {
        return _memoryCache.GetOrCreate(
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
    }

    private string GetJson() => File.Exists(Path)
        ? File.ReadAllText(Path)
        : "{}";

    public IEnumerable<Bookmark> GetBookmarks() => FetchAll();

    public IEnumerable<Bookmark> GetBookmarks(string filter) => FetchAll().Where(e => e.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase));

    public bool IsBookmarkSourceAvailable() => File.Exists(Path);

    #endregion
}