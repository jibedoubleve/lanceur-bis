using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Humanizer;
using Lanceur.Core.Repositories.Config;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.Extensions;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Lanceur.Ui.WPF.Views;

public partial class SettingsView : INavigationWindow
{
    #region Fields

    private readonly IContentDialogService _contentDialogService;
    private readonly ILogger<SettingsView> _logger;
    private readonly ISettingsFacade _settings;
    private readonly ISnackbarService _snackbarService;

    #endregion

    #region Constructors

    public SettingsView(
        SettingsViewModel viewModel,
        IContentDialogService contentDialogService,
        ISnackbarService snackbarService,
        IServiceProvider serviceProvider,
        ILogger<SettingsView> logger,
        ISettingsFacade settings
    )
    {
        ArgumentNullException.ThrowIfNull(contentDialogService);
        ArgumentNullException.ThrowIfNull(snackbarService);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        DataContext = viewModel;

        InitializeComponent();

        PageNavigationView.SetServiceProvider(serviceProvider);
        _contentDialogService = contentDialogService;
        _snackbarService = snackbarService;
        _logger = logger;
        _settings = settings;
        contentDialogService.SetDialogHost(ContentPresenterForDialogs);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);

        WeakReferenceMessenger.Default.Register<SettingsView, NotificationMessage>(this, (_, m) => Notify(m));
        WeakReferenceMessenger.Default.Register<SettingsView, NavigationMessage>(this, (_, m) => NavigateTo(m));
        WeakReferenceMessenger.Default.Register<SettingsView, QuestionRequestMessage>(this, (_, m) => m.Reply(HandleMessageBoxAsync(m)));
    }

    #endregion

    #region Methods

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

    private static ControlAppearance MapAppearance(MessageLevel level) => level switch
    {
        MessageLevel.Success => ControlAppearance.Success,
        MessageLevel.Warning => ControlAppearance.Caution,
        _                    => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };

    private static IconElement MapIcon(MessageLevel level)
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
            15.Seconds()
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

    private void OnLoaded(object _, RoutedEventArgs e)
    {
        SystemThemeWatcher.Watch(
            this,
            _settings.Application.Window.BackdropStyle.ToWindowBackdropType(),
            updateAccents: true
        );
    }

    /// <inheritdoc />
    public void CloseWindow() => Close();

    /// <inheritdoc />
    public INavigationView GetNavigation() => PageNavigationView;

    public void Navigate<T>(object? dataContext = null) where T : Page => PageNavigationView.Navigate(typeof(T), dataContext);

    /// <inheritdoc />
    public bool Navigate(Type pageType) => PageNavigationView.Navigate(pageType);

    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => PageNavigationView.SetPageProviderService(navigationViewPageProvider);

    /// <inheritdoc />
    public void SetServiceProvider(IServiceProvider serviceProvider) => _logger.LogWarning("Method '{Method}' is not implemented", nameof(SetServiceProvider));

    /// <inheritdoc />
    public void ShowWindow() => Show();

    #endregion
}