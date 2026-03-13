using Lanceur.Core.Configuration;
using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Interaction logic for StoreOverride.xaml
/// </summary>
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