namespace System.Web.Bookmarks.RepositoryConfiiguration;

public interface IChromiumBrowserConfiguration
{
    #region Properties

    string CacheKey { get; }
    string Path { get; }

    #endregion
}