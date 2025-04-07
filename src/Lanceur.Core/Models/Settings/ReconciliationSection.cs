namespace Lanceur.Core.Models.Settings;

/// <summary>
///     Represents configuration settings used by the reconciliation tools.
/// </summary>
public class ReconciliationSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the inactivity threshold (in months).
    ///     This value determines the number of months of inactivity after which an alias is considered inactive.
    ///     The default value is 6 months.
    /// </summary>
    public int InactivityThreshold { get; set; } = 6;

    /// <summary>
    ///     Gets or sets the threshold below which an alias is considered to have low usage.
    /// </summary>
    public int LowUsageThreshold { get; set; } = 10;

    #endregion
}