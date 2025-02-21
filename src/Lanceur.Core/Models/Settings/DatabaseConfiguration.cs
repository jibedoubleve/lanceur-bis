﻿namespace Lanceur.Core.Models.Settings;

/// <summary>
///     Represents the configuration settings for the application.
///     These settings are stored directly within the database and define key behaviours and preferences.
/// </summary>
public class DatabaseConfiguration
{
    #region Properties

    /// <summary>
    ///     Gets or sets the caching configuration settings used across the application.
    /// </summary>
    public CachingSession Caching { get; set; } = CachingSession.Default;

    /// <summary>
    ///     Gets or sets the hotkey configuration for displaying the search window.
    ///     This setting allows users to customise the shortcut key for quick access.
    /// </summary>
    public HotKeySection HotKey { get; set; } = HotKeySection.Default;

    /// <summary>
    ///     Gets or sets the configuration settings for the search box.
    /// </summary>
    public SearchBoxSection SearchBox { get; set; } = SearchBoxSection.Default;

    /// <summary>
    ///     Gets or sets the configuration settings for the store section.
    ///     This includes preferences and metadata related to connected stores or sources.
    /// </summary>
    public StoreSection Stores { get; set; } = StoreSection.Default;

    /// <summary>
    ///     Gets or sets the configuration settings for the application's main window.
    ///     This includes dimensions, position, and other display-related preferences.
    /// </summary>
    public WindowSection Window { get; set; } = WindowSection.Default;

    #endregion
}