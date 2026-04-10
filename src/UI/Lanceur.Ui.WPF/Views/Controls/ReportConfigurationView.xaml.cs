using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Controls;

namespace Lanceur.Ui.WPF.Views.Controls;

[ExcludeFromIoC]
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