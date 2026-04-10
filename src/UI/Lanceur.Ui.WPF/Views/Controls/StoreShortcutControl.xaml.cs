using Lanceur.Core.Models;
using Lanceur.SharedKernel.IoC;

namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Interaction logic for StoreOverride.xaml
/// </summary>
[ExcludeFromIoC]
public partial class StoreShortcutControl
{
    #region Constructors

    public StoreShortcutControl(StoreShortcut viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}