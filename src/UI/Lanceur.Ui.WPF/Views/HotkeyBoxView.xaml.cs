using System.Windows;
using Lanceur.Ui.Core.ViewModels;

namespace Lanceur.Ui.WPF.Views;

public partial class HotkeyBoxView
{
    #region Constructors

    public HotkeyBoxView(HotkeyBoxViewModel viewModel)
    {
        DataContext = ViewModel = viewModel;
        InitializeComponent();
    }

    #endregion

    #region Properties

    public HotkeyBoxViewModel ViewModel { get; }

    #endregion

    #region Methods

    private void Close(bool dialogueResult)
    {
        DialogResult = dialogueResult;
        Close();
    }

    private void OnClickCancel(object sender, RoutedEventArgs e) => Close(false);

    private void OnClickOk(object sender, RoutedEventArgs e) => Close(true);

    #endregion
}