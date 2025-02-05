namespace System.Web.Bookmarks.RepositoryConfiguration;

/// <inheritdoc />
internal class ZenConfiguration : IGeckoBrowserConfiguration
{
    #region Properties

    /// <inheritdoc />
    public string CacheKey => CacheKeys.Zen;

    /// <inheritdoc />
    public string Database => @"%AppData%\zen\{0}\places.sqlite";

    /// <inheritdoc />
    public string IniFilename =>  @"%AppData%\zen\profiles.ini";

    #endregion
}