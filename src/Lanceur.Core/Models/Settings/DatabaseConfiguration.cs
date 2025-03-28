using Lanceur.Core.Constants;

namespace Lanceur.Core.Models.Settings;

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
    public CachingSession Caching { get; } = new();

    /// <summary>
    ///     Get or sets the configuration settings for the feature flags.
    /// </summary>
    public IEnumerable<FeatureFlag> FeatureFlags { get; set; } =
    [
        new()
        {
            FeatureName = Features.ResourceDisplay,
            Enabled = true,
            Description = "Show CPU and Memory Usage in Search Box",
            Icon = "Gauge24"
        }
    ];

    /// <summary>
    ///     Gets or sets the hotkey configuration for displaying the search window.
    ///     This setting allows users to customise the shortcut key for quick access.
    /// </summary>
    /// <remarks>
    ///     The default hotkey configuration: ALTGR + Space.
    ///     The values 3 and 18 represent the key codes for the ALTGR and Space keys, respectively.
    /// </remarks>
    public HotKeySection HotKey { get; } = new(3, 18);

    /// <summary>
    ///     Gets or sets the configuration settings for the search box.
    /// </summary>
    public SearchBoxSection SearchBox { get; } = new();

    /// <summary>
    ///     Gets or sets the configuration settings for the store section.
    ///     This includes preferences and metadata related to connected stores or sources.
    /// </summary>
    public StoreSection Stores { get; } = new();

    /// <summary>
    ///     Gets or sets the configuration settings for the application's main window.
    ///     This includes dimensions, position, and other display-related preferences.
    /// </summary>
    public WindowSection Window { get; } = new();

    #endregion
}

public static class DatabaseConfigurationExtensions
{
    #region Methods

    public static void SetHotKey(this DatabaseConfiguration databaseConfiguration, int key, int modifierKey)
    {
        databaseConfiguration.HotKey.Key = key;
        databaseConfiguration.HotKey.ModifierKey = modifierKey;
    }

    #endregion
}