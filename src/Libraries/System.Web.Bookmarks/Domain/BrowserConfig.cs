namespace System.Web.Bookmarks.Domain;

public struct BrowserConfig
{
    #region Properties

    public static (string Database, string IniFilename) Firefox => new(@"%AppData%\Mozilla\Firefox\{0}\places.sqlite", @"%AppData%\Mozilla\Firefox\profiles.ini");
    public static (string Database, string IniFilename ) Zen => new(@"%AppData%\zen\{0}\places.sqlite", @"%AppData%\zen\profiles.ini");

    #endregion
}