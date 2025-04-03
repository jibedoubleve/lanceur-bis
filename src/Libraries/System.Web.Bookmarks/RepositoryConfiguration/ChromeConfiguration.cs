using Lanceur.SharedKernel.Extensions;

namespace System.Web.Bookmarks.RepositoryConfiguration;

public class ChromeConfiguration : IBlinkBrowserConfiguration
{
    #region Properties

    public string CacheKey => CacheKeys.Chrome;
    public string Path => @"%LOCALAPPDATA%\Google\Chrome\User Data\Default\Bookmarks".ExpandPath();

    #endregion
}