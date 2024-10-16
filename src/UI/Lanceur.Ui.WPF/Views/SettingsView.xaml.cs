using System.ComponentModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Ui.Core.Messages;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Views;

public partial class SettingsView
{
    #region Fields

    private readonly ISnackbarService _snackbarService;

    #endregion

    #region Constructors

    public SettingsView(
        IPageService pageService,
        IContentDialogService contentDialogService,
        ISnackbarService snackbarService
    )
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
        WeakReferenceMessenger.Default.Register<SettingsView, NavigationMessage>(this, (_, m) => NavigateTo(m));
    }

    #endregion

    #region Methods

    private ControlAppearance MapAppearance(MessageLevel level) => level switch
    {
        MessageLevel.Success => ControlAppearance.Success,
        MessageLevel.Warning => ControlAppearance.Caution,
        _                    => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };

    private IconElement MapIcon(MessageLevel level)
    {
        var icon = level switch
        {
            MessageLevel.Success => SymbolRegular.Checkmark24,
            MessageLevel.Warning => SymbolRegular.Warning24,
            _                    => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
        return new SymbolIcon(icon);
    }

    private void NavigateTo(NavigationMessage message) => PageNavigationView.Navigate(message.Value);

    private void Notify(NotificationMessage message)
    {
        _snackbarService.Show(
            message.Value.Title,
            message.Value.Message,
            MapAppearance(message.Value.Level),
            MapIcon(message.Value.Level),
            5.Seconds()
        );
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    public void Navigate<T>() where T : Page => PageNavigationView.Navigate(typeof(T));

    #endregion
}