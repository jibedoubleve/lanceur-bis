namespace System.Web.Bookmarks.RepositoryConfiguration;

public interface IChromiumBrowserConfiguration
{
    #region Properties

    string CacheKey { get; }
    string Path { get; }

    #endregion
}