using Lanceur.SharedKernel.Extensions;

namespace System.Web.Bookmarks.RepositoryConfiguration;

internal class EdgeConfiguration : IBlinkBrowserConfiguration
{
    #region Properties

    public string CacheKey => CacheKeys.Edge;
    public string Path => @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Bookmarks".ExpandPath();

    #endregion
}