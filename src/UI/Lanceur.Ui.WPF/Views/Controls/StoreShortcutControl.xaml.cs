using Lanceur.Core.Models.Settings;

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