namespace System.Web.Bookmarks.RepositoryConfiguration;

internal sealed class DummyBlinkConfiguration : IBlinkBrowserConfiguration
{
    #region Properties

    public string CacheKey => Guid.NewGuid().ToString();
    public string Path => IO.Path.GetRandomFileName();

    #endregion
}