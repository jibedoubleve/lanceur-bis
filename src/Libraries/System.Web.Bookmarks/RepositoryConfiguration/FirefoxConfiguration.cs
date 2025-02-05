namespace System.Web.Bookmarks.RepositoryConfiguration;

/// <inheritdoc />
internal class FirefoxConfiguration : IGeckoBrowserConfiguration
{
    #region Properties

    /// <inheritdoc />
    public string CacheKey => CacheKeys.Firefox;

    /// <inheritdoc />
    public string Database => @"%AppData%\Mozilla\Firefox\{0}\places.sqlite";

    /// <inheritdoc />
    public string IniFilename => @"%AppData%\Mozilla\Firefox\profiles.ini";

    #endregion
}