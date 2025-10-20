using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;

namespace Lanceur.Core.Configuration.Configurations;

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
    public CachingSection Caching { get; set; } = new(30, 30);

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
        },
        new()
        {
            FeatureName = Features.AdditionalParameterAlwaysActive,
            Enabled = false,
            Description = "Display additional parameter results without requiring ':' input",
            Icon = "PanelSeparateWindow20"
        }
    ];

    public GithubSection Github { get; set; } = new();

    /// <summary>
    ///     Gets or sets the hotkey configuration for displaying the search window.
    ///     This setting allows users to customise the shortcut key for quick access.
    /// </summary>
    /// <remarks>
    ///     The default hotkey configuration: ALTGR + Space.
    ///     The values 3 and 18 represent the key codes for the ALTGR and Space keys, respectively.
    /// </remarks>
    public HotKeySection HotKey { get; set; } = new(3, 18);

    /// <summary>
    ///     Gets or sets miscellaneous configuration settings that do not fall into other categories.
    ///     These settings may include other application preferences not specifically handled elsewhere.
    /// </summary>
    public ReconciliationSection Reconciliation { get; set; } = new(6, 10);

    /// <summary>
    ///     Contains settings for the resource monitor.
    ///     This section includes configuration options related to system resource monitoring,
    ///     such as CPU, memory, and other performance metrics.
    /// </summary>
    public ResourceMonitorSection ResourceMonitor { get; set; } = new(10, 500);

    /// <summary>
    ///     Gets or sets the configuration settings for the search box.
    /// </summary>
    public SearchBoxSection SearchBox { get; set; } = new();

    /// <summary>
    ///     Gets or sets the configuration settings for the store section.
    ///     This includes preferences and metadata related to connected stores or sources.
    /// </summary>
    public StoreSection Stores { get; set; } = new();

    /// <summary>
    ///     Gets or sets the configuration settings for the application's main window.
    ///     This includes dimensions, position, and other display-related preferences.
    /// </summary>
    public WindowSection Window { get; set; } = new();

    #endregion
}