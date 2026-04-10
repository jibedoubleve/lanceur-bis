using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Controls;

namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Interaction logic for NumericSelectorControl.xaml
/// </summary>
[ExcludeFromIoC]
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