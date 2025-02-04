namespace System.Web.Bookmarks.RepositoryConfiiguration;

internal struct BrowserConfigurationFactory
{
    #region Properties

    public static IChromiumBrowserConfiguration Chrome => new ChromeConfiguration();

    public static IChromiumBrowserConfiguration Edge => new EdgeConfiguration();

    public static IGeckoBrowserConfiguration Firefox => new FirefoxConfiguration();
    
    public static IGeckoBrowserConfiguration Zen => new ZenConfiguration();

    #endregion
}