using System.Web.Bookmarks.Domain;

namespace System.Web.Bookmarks.Configuration;

/// <inheritdoc />
public class FirefoxConfiguration : IGeckoBrowserConfiguration
{
    #region Properties

    /// <inheritdoc />
    public string CacheKey => "Firefox";

    /// <inheritdoc />
    public string Database => @"%AppData%\Mozilla\Firefox\{0}\places.sqlite";

    /// <inheritdoc />
    public string IniFilename => @"%AppData%\Mozilla\Firefox\profiles.ini";

    #endregion
}