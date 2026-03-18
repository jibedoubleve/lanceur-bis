using Lanceur.Core.Constants;
using Lanceur.Core.Models;

namespace Lanceur.Core.Configuration.Sections.Application;

/// <summary>
///     Represents configuration settings used by the reconciliation tools.
/// </summary>
public sealed class ReconciliationSection
{
    #region Constructors

    public ReconciliationSection(int inactivityThreshold, int lowUsageThreshold)
    {
        InactivityThreshold = inactivityThreshold;
        LowUsageThreshold = lowUsageThreshold;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the inactivity threshold (in months).
    ///     This value determines the number of months of inactivity after which an alias is considered inactive.
    ///     The default value is 6 months.
    /// </summary>
    public int InactivityThreshold { get; set; }

    /// <summary>
    ///     Gets or sets the threshold below which an alias is considered to have low usage.
    /// </summary>
    public int LowUsageThreshold { get; set; }

    /// <summary>
    ///     Gets or sets the configuration settings used to control the display of columns
    ///     in various reconciliation reports. Each <see cref="ReportsConfiguration" />
    ///     entry defines user-specific preferences for a particular report type,
    ///     such as which columns should be visible in the report view.
    /// </summary>

    public IEnumerable<ReportConfiguration> ReportsConfiguration { get; set; } =
    [
        new(
            ReportType.RestoreAlias, // A.K.A. logically deleted aliases...
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = false,
                UsageCount = false,
                Parameters = true,
                FileName = true
            }
        ),
        new(
            ReportType.UnusedAliases,
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = false,
                UsageCount = false,
                Parameters = true,
                FileName = true
            }
        ),
        new(
            ReportType.InactiveAliases,
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = true,
                UsageCount = false,
                Parameters = false,
                FileName = false
            }
        ),
        new(
            ReportType.RarelyUsedAliases,
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = true,
                UsageCount = false,
                Parameters = false,
                FileName = false
            }
        ),
        new(
            ReportType.UnannotatedAliases,
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = false,
                UsageCount = false,
                Parameters = false,
                FileName = true
            }
        ),
        new(
            ReportType.DoubloonAliases,
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = false,
                UsageCount = false,
                Parameters = true,
                FileName = true
            }
        ),
        new(
            ReportType.BrokenAliases,
            new ColumnsConfiguration
            {
                ProposedDescription = false,
                LastUsed = false,
                UsageCount = false,
                Parameters = true,
                FileName = true
            }
        )
    ];

    #endregion
}

public sealed class ReportConfiguration(ReportType reportType, ColumnsConfiguration columnsVisibility) : ObservableModel
{
    #region Fields

    private ColumnsConfiguration _columnsVisibility = columnsVisibility;
    private ReportType _reportType = reportType;

    #endregion

    #region Properties

    public ColumnsConfiguration ColumnsVisibility
    {
        get => _columnsVisibility;
        set => SetField(ref _columnsVisibility, value);
    }

    public ReportType ReportType
    {
        get => _reportType;
        set => SetField(ref _reportType, value);
    }

    #endregion
}

public sealed class ColumnsConfiguration : ObservableModel
{
    #region Fields

    private bool _fileName;
    private bool _lastUsed;
    private bool _parameters;
    private bool _proposedDescription;
    private bool _usageCount;

    #endregion

    #region Properties

    public bool FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    public bool LastUsed
    {
        get => _lastUsed;
        set => SetField(ref _lastUsed, value);
    }

    public bool Parameters
    {
        get => _parameters;
        set => SetField(ref _parameters, value);
    }

    public bool ProposedDescription
    {
        get => _proposedDescription;
        set => SetField(ref _proposedDescription, value);
    }

    public bool UsageCount
    {
        get => _usageCount;
        set => SetField(ref _usageCount, value);
    }

    #endregion
}