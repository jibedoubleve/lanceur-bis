namespace System.Web.Bookmarks.Domain;

public record Bookmark
{
    #region Properties

    public string Name { get; init; } = string.Empty;
    public string SortKey { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;

    #endregion
}