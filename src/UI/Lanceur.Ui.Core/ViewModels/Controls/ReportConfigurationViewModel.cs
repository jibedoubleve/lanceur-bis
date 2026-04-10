using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.SharedKernel.IoC;

namespace Lanceur.Ui.Core.ViewModels.Controls;

[ExcludeFromIoC]
public partial class ReportConfigurationViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<ReportConfiguration> _configurations;
    [ObservableProperty] private string _label;
    [ObservableProperty] private ReportConfiguration _selectedConfiguration;
    [ObservableProperty] private string _toolTip;

    #endregion

    #region Constructors

    /// <inheritdoc />
    public ReportConfigurationViewModel(
        IEnumerable<ReportConfiguration> configurations,
        string label,
        string toolTip,
        ReportType currentReport
    )
    {
        Configurations = [.. configurations.Select(c => c.Clone())];
        SelectedConfiguration = Configurations.FirstOrDefault(e => e.ReportType == currentReport)!;
        Label = label;
        ToolTip = toolTip;
    }

    #endregion
}