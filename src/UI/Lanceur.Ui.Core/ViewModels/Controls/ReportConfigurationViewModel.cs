using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Configuration;
using Lanceur.Core.Constants;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class ReportConfigurationViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<ReportConfiguration> _configurations  ;
    [ObservableProperty] private string _label  ;
    [ObservableProperty] private ReportConfiguration _selectedConfiguration;
    [ObservableProperty] private string _toolTip  ;

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
        Configurations = new(configurations);
        SelectedConfiguration = Configurations.FirstOrDefault(e => e.ReportType == currentReport)!;
        Label = label;
        ToolTip = toolTip;
    }

    #endregion
}