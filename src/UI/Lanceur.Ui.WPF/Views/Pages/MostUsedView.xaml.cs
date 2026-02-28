using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class MostUsedView
{
    #region Constructors

    public MostUsedView(MostUsedViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}