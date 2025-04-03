namespace System.Web.Bookmarks.RepositoryConfiguration;

internal struct BrowserConfigurationFactory
{
    #region Properties

    public static IBlinkBrowserConfiguration Chrome => new ChromeConfiguration();

    public static IBlinkBrowserConfiguration Edge => new EdgeConfiguration();

    public static IGeckoBrowserConfiguration Firefox => new FirefoxConfiguration();
    
    public static IGeckoBrowserConfiguration Zen => new ZenConfiguration();

    #endregion
}