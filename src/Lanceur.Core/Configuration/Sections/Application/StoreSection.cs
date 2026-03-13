using Lanceur.Core.Models;

namespace Lanceur.Core.Configuration.Sections;

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
    ///     This suffix can be used to modify search behaviour, such as excluding system or hidden files.
    /// </summary>
    public string EverythingQuerySuffix { get; set; } = string.Empty;

    /// <summary>
    ///     Represents a user-defined alias that replaces the default shortcut
    ///     to perform a search using the store associated with that shortcut.
    /// </summary>
    public IEnumerable<StoreShortcut> StoreShortcuts { get; set; } = new List<StoreShortcut>();

    #endregion
}

public static class StoreSectionExtension
{
    #region Methods

    /// <summary>
    ///     Returns the alias override configured for the specified store type,
    ///     or an empty string if no override is defined.
    /// </summary>
    /// <param name="storeSection">The store configuration section containing the shortcut overrides.</param>
    /// <param name="storeType">The store instance whose type is used to look up the override.</param>
    /// <returns>The alias override for the store, or <see cref="string.Empty"/> if none is found.</returns>
    public static string GetOverride(this StoreSection storeSection, object storeType) =>
        storeSection.StoreShortcuts
                    .FirstOrDefault(x => storeType.GetType().FullName!.Equals(x.StoreType))
                    ?.AliasOverride ?? string.Empty;

    #endregion
}