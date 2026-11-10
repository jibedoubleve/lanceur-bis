using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Controls;

namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Interaction logic for StoreShortcutControl.xaml
/// </summary>
[ExcludeFromIoC]
public partial class StoreShortcutControl
{
    #region Constructors

    public StoreShortcutControl(StoreShortcutSelectorViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}