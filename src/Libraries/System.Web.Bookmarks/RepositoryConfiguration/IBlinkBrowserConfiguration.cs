namespace System.Web.Bookmarks.RepositoryConfiguration;

public interface IBlinkBrowserConfiguration
{
    #region Properties

    string CacheKey { get; }
    string Path { get; }

    #endregion
}