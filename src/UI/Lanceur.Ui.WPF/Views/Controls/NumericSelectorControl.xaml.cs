using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Interaction logic for NumericSelectorControl.xaml
/// </summary>
public partial class NumericSelectorControl
{
    #region Constructors

    public NumericSelectorControl(NumericSelectorViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}