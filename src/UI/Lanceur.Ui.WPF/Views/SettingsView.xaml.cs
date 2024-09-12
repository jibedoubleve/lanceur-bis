using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.WPF.Views.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Views;

public partial class SettingsView
{
    private readonly ISnackbarService _snackbarService;

    #region Constructors

    public SettingsView(IPageService pageService, 
                        IContentDialogService contentDialogService, 
                        ISnackbarService snackbarService)
    {
        ArgumentNullException.ThrowIfNull(pageService);
        ArgumentNullException.ThrowIfNull(contentDialogService);
        ArgumentNullException.ThrowIfNull(snackbarService);
        
        InitializeComponent();
        _snackbarService = snackbarService;
        PageNavigationView.SetPageService(pageService);
        contentDialogService.SetDialogHost(ContentPresenterForDialogs);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        
        WeakReferenceMessenger.Default.Register<SettingsView, SuccessMessage>(this, (_, m) => NotifySuccess(m));
    }

    private void NotifySuccess(SuccessMessage message)
    {
        _snackbarService.Show(
            message.Value.Item1,
            message.Value.Item2,
            ControlAppearance.Success,
            new SymbolIcon(SymbolRegular.Checkmark24),
            4.Seconds());
    }

    #endregion

    #region Methods

    public void Navigate<T>() where T : Page => PageNavigationView.Navigate(typeof(KeywordsView));

    #endregion

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}