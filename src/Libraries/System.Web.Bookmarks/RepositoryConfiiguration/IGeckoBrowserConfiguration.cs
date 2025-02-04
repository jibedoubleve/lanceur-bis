namespace System.Web.Bookmarks.RepositoryConfiiguration;

/// <summary>
///     Defines configuration settings related to bookmarks for the browser.
/// </summary>
public interface IGeckoBrowserConfiguration
{
    #region Properties

    /// <summary>
    ///     Gets the prefix used for memory caching. This prefix helps isolate the cache for this browser
    ///     from caches used by other browsers or components.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    ///     Gets the file path to the database that stores the bookmarks.
    ///     This database contains all the saved bookmark entries.
    /// </summary>
    string Database { get; }

    /// <summary>
    ///     Gets the file path to the INI file that stores configuration settings for bookmarks.
    ///     This file defines various settings and preferences related to bookmark management.
    /// </summary>
    string IniFilename { get; }

    #endregion
}