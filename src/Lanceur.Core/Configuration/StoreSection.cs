namespace Lanceur.Core.Configuration;

/// <summary>
///     Represents a section of the store configuration, providing properties
///     for managing browser bookmark retrieval settings.
/// </summary>
public class StoreSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the name of the installed browser from which
    ///     bookmarks are retrieved. This property is used to identify
    ///     the source browser for bookmark management.
    /// </summary>
    public string BookmarkSourceBrowser { get; set; } = "Chrome";

    /// <summary>
    ///     Gets or sets the suffix appended to queries executed with the Everything search engine.
    ///     This suffix can be used to modify search behavior, such as excluding system or hidden files.
    /// </summary>
    public string EverythingQuerySuffix { get; set; } = string.Empty;

    /// <summary>
    ///     Represents a user-defined alias that replaces the default shortcut
    ///     to perform a search using the store associated with that shortcut.
    /// </summary>
    public IEnumerable<StoreShortcut> StoreShortcuts { get; set; } = new List<StoreShortcut>();

    #endregion
}