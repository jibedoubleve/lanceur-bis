using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;

namespace System.Web.Bookmarks;

public class ChromeBookmarksRepository : IBookmarkRepository
{
    #region Fields

    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = $"ChromeBookmarks_{nameof(ChromeBookmarksRepository)}";

    private static readonly string Path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Bookmarks");

    #endregion

    #region Constructors

    public ChromeBookmarksRepository(IMemoryCache memoryCache)
    {
        ArgumentNullException.ThrowIfNull(memoryCache);
        _memoryCache = memoryCache;
    }

    #endregion

    #region Methods

    private void FetchAll(JsonNode? jsonNode, List<Bookmark> results)
    {
        if (jsonNode is null) return;

        if (jsonNode is JsonArray array)
        {
            foreach (var item in array) FetchAll(item, results);
            return;
        }

        if (jsonNode is JsonObject jsonObject)
        {
            if (jsonObject.ContainsKey("children"))
            {
                FetchAll(jsonObject["children"], results);
                return;
            }

            var name = jsonObject["name"]?.ToString();
            var url = jsonObject["url"]?.ToString();
            var order = jsonObject["date_last_used"]?.ToString() ?? "0";

            if (name is not null && url is not null) results.Add(new(name, url, order));
        }
    }

    private IEnumerable<Bookmark>  FetchAll()
    {
        return _memoryCache.GetOrCreate(
            CacheKey,
            IEnumerable<Bookmark> (_) =>
            {
                var json = File.ReadAllText(Path);
                var node = JsonNode.Parse(json);
                var results = new List<Bookmark>();

                var startNode = node?["roots"]?["bookmark_bar"];
                if (startNode is not null) FetchAll(startNode, results);
                return results.OrderByDescending(e => e.Order);
            }
        )!;
    }

    public IEnumerable<Bookmark> GetBookmarks() => FetchAll();

    public IEnumerable<Bookmark> GetBookmarks(string filter) => FetchAll()
        .Where(
            e => e.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
        );

    #endregion
}