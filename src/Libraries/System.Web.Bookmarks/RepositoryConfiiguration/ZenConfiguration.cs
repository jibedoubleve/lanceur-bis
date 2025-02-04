namespace System.Web.Bookmarks.RepositoryConfiiguration;

/// <inheritdoc />
internal class ZenConfiguration : IGeckoBrowserConfiguration
{
    #region Properties

    /// <inheritdoc />
    public string CacheKey => "Zen";

    /// <inheritdoc />
    public string Database => @"%AppData%\zen\{0}\places.sqlite";

    /// <inheritdoc />
    public string IniFilename =>  @"%AppData%\zen\profiles.ini";

    #endregion
}