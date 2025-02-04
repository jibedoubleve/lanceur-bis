using Lanceur.SharedKernel.Extensions;

namespace System.Web.Bookmarks.RepositoryConfiiguration;

internal class EdgeConfiguration : IChromiumBrowserConfiguration
{
    #region Properties

    public string CacheKey => $"EdgeBookmarks_{nameof(EdgeConfiguration)}";
    public string Path => @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Bookmarks".ExpandPath();

    #endregion
}

public class ChromeConfiguration : IChromiumBrowserConfiguration
{
    #region Properties

    public string CacheKey => $"ChromeBookmarks_{nameof(ChromeConfiguration)}";
    public string Path => @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Bookmarks".ExpandPath();

    #endregion
}