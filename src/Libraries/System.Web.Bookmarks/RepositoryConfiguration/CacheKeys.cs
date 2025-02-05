namespace System.Web.Bookmarks.RepositoryConfiguration;

internal static class CacheKeys
{
    #region Fields

    public static readonly string Chrome;
    public static readonly string Edge;
    public static readonly string Firefox;
    public static readonly string Zen;
    private static readonly Dictionary<string, string> Dictionary;

    #endregion

    #region Constructors

    static CacheKeys()
    {
        Dictionary = new()
        {
            [nameof(Chrome)] = "Chrome_Bookmarks_CacheKey",
            [nameof(Edge)] = "Edge_Bookmarks_CacheKey",
            [nameof(Firefox)] = "Firefox_Bookmarks_CacheKey",
            [nameof(Zen)] = "Zen_Bookmarks_CacheKey"
        };
        Chrome = Dictionary![nameof(Chrome)];
        Edge = Dictionary![nameof(Edge)];
        Firefox = Dictionary![nameof(Firefox)];
        Zen = Dictionary![nameof(Zen)];
    }

    #endregion

    #region Properties

    public static string[] Keys => Dictionary.Keys.ToArray();

    #endregion
}