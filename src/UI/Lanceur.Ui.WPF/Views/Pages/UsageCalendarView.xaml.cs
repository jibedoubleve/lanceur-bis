using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class UsageCalendarView
{
    #region Constructors

    public UsageCalendarView(UsageCalendarViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}