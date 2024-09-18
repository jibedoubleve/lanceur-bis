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
        
        WeakReferenceMessenger.Default.Register<SettingsView, NotificationMessage>(this, (_, m) => Notify(m));
    }

    private void Notify(NotificationMessage message)
    {
        _snackbarService.Show(
            message.Value.Item2,
            message.Value.Item3,
            MapAppearance(message.Value.Item1),
            MapIcon(message.Value.Item1),
            5.Seconds());
    }

    private IconElement MapIcon(MessageLevel level)
    {
        var icon = level switch
        {
            MessageLevel.Success => SymbolRegular.Checkmark24,
            MessageLevel.Warning => SymbolRegular.Warning24,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
        return new SymbolIcon(icon);
    }

    private ControlAppearance MapAppearance(MessageLevel level) => level switch
    {
        MessageLevel.Success => ControlAppearance.Success,
        MessageLevel.Warning => ControlAppearance.Caution,
        _                    => throw new ArgumentOutOfRangeException(nameof(level), level, null),
    };

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