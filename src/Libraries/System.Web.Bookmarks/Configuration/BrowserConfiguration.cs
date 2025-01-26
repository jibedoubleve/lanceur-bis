using System.Web.Bookmarks.Domain;

namespace System.Web.Bookmarks.Configuration;

public struct BrowserConfiguration
{
    #region Properties

    public static IGeckoBrowserConfiguration Firefox => new FirefoxConfiguration();
    public static IGeckoBrowserConfiguration Zen => new ZenConfiguration();

    #endregion
}