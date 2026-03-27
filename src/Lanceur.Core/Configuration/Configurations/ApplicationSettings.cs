using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;

namespace Lanceur.Core.Configuration.Configurations;

/// <summary>
///     Represents the configuration settings for the application.
///     These settings are stored directly within the database and define key behaviours and preferences.
/// </summary>
public class ApplicationSettings
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
        },
        new()
        {
            FeatureName = Features.SteamIntegration,
            Enabled = false,
            Description = "Browse and launch games installed in the Steam library",
            Icon = "Games24"
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

public static class ApplicationSettingsExtensions
{
    #region Methods

    private static void RemoveStaleFeatureFlags(List<FeatureFlag> currentFf)
    {
        var ffNames = Features.GetNames();
        var toRemove = currentFf.Where(f => !ffNames.Contains(f.FeatureName))
                                .ToArray();

        foreach (var f in toRemove) { currentFf.Remove(f); }
    }

    /// <summary>
    ///     Synchronises the feature flags in <paramref name="config" /> with the current set of known flags:
    ///     <list type="bullet">
    ///         <item>Flags present in code but missing from the database are added (new features).</item>
    ///         <item>Flags present in the database but no longer in code are removed (removed features).</item>
    ///         <item>Flags already in the database are left untouched (user preferences preserved).</item>
    ///     </list>
    /// </summary>
    /// <remarks>
    ///     The result is assigned as a <c>FeatureFlag[]</c> (array) on purpose. Keeping it as an array
    ///     prevents Newtonsoft.Json from appending items on subsequent <c>PopulateObject</c> calls
    ///     (arrays are fixed-size, so Newtonsoft replaces them rather than merging into them).
    ///     Changing <c>ToArray()</c> to <c>ToList()</c> would reintroduce a duplication bug on reload.
    /// </remarks>
    public static void ReconcileFeatureFlags(this ApplicationSettings config)
    {
        var defaultFf = new ApplicationSettings().FeatureFlags;
        var currentFf = new List<FeatureFlag>(config.FeatureFlags);

        // Add flags that exist in code but are not yet in the database
        var newFf = defaultFf.Where(f => config.FeatureFlags.All(c => c.FeatureName != f.FeatureName));
        currentFf.AddRange(newFf);

        // Remove flags that no longer exist in code
        RemoveStaleFeatureFlags(currentFf);

        config.FeatureFlags = currentFf.ToArray();
    }

    #endregion
}