using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Ui.Core.Messages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Lanceur.Ui.WPF.Views;

public partial class SettingsView
{
    #region Fields

    private readonly IContentDialogService _contentDialogService;
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
        _contentDialogService = contentDialogService;
        _snackbarService = snackbarService;
        PageNavigationView.SetPageService(pageService);
        contentDialogService.SetDialogHost(ContentPresenterForDialogs);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        WeakReferenceMessenger.Default.Register<SettingsView, NotificationMessage>(this, (_, m) => Notify(m));
        WeakReferenceMessenger.Default.Register<SettingsView, NavigationMessage>(this, (_, m) => NavigateTo(m));
        WeakReferenceMessenger.Default.Register<SettingsView, QuestionRequestMessage>(this, (_, m) => m.Reply(HandleMessageBoxAsync(m)));
    }

    private async Task<bool> HandleMessageBoxAsync(QuestionRequestMessage request)
    {
        var result = await _contentDialogService.ShowSimpleDialogAsync(
            new()
            {
                Title = request.Title,
                Content = request.Content,
                PrimaryButtonText = request.YesText,
                CloseButtonText = request.NoText
            }
        );
        return result == ContentDialogResult.Primary;
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

    private void NavigateTo(NavigationMessage message) => PageNavigationView.Navigate(message.Value.ViewType, message.Value.DataContext);

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

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    public void Navigate<T>(object? dataContext = null) where T : Page => PageNavigationView.Navigate(typeof(T), dataContext);

    #endregion
}