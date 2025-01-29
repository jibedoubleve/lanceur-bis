namespace Lanceur.Core.Models.Settings;

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
    ///     Gets a default instance of the <see cref="StoreSection" /> class
    ///     with default values. Use this as a baseline configuration.
    /// </summary>
    public static StoreSection Default => new();

    /// <summary>
    ///     Represents a user-defined alias that replaces the default shortcut
    ///     to perform a search using the store associated with that shortcut.
    /// </summary>
    public IEnumerable<StoreShortcut> StoreOverrides { get; set; }

    #endregion
}