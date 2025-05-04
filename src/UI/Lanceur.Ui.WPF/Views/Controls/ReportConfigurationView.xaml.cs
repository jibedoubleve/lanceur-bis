using Lanceur.Ui.Core.ViewModels.Controls;

namespace Lanceur.Ui.WPF.Views.Controls;

public partial class ReportConfigurationView
{
    #region Constructors

    public ReportConfigurationView(ReportConfigurationViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}