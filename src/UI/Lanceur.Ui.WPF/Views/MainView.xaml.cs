using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.Extensions;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.Services;
using Lanceur.Ui.WPF.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHotkey;
using Wpf.Ui.Appearance;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Lanceur.Ui.WPF.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView
{
    #region Fields

    private readonly IDatabaseConfigurationService _databaseConfig;
    private readonly IHotKeyService _hotKeyService;
    private readonly ILogger<MainView> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public MainView(
        MainViewModel viewModel,
        ILogger<MainView> logger,
        IServiceProvider serviceProvider,
        ISettingsFacade settings,
        IHotKeyService hotKeyService,
        IDatabaseConfigurationService databaseConfig
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(hotKeyService);
        ArgumentNullException.ThrowIfNull(databaseConfig);

        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings;
        _hotKeyService = hotKeyService;
        _databaseConfig = databaseConfig;

        DataContext = viewModel;

        InitializeComponent();

        var messenger = WeakReferenceMessenger.Default;
        messenger.Register<KeepAliveMessage>(
            this,
            (_, m) =>
            {
                if (m.Value)
                    ShowWindow();
                else
                    HideWindow();
            }
        );
        messenger.Register<ChangeCoordinateMessage>(this, (_, m) => SetWindowPosition(m.Value));
        messenger.Register<SetQueryMessage>(this, (_, m) => SetQuery(m.Value));
    }

    #endregion

    #region Properties

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    #endregion

    #region Methods

    private void HideWindow()
    {
        if (ViewModel.ShowLastQuery)
            QueryTextBox.SelectAll();
        else
            QueryTextBox.Clear();
        Hide();
    }

    private void OnClickDarkTheme(object sender, RoutedEventArgs e)
    {
        var windowBackdropType = _settings.Application.Window.BackdropStyle.ToWindowBackdropType();
        _logger.LogTrace("Change theme to {Theme} and backdrop type {BackdropType}", ApplicationTheme.Dark, windowBackdropType);
        ApplicationThemeManager.Apply(ApplicationTheme.Dark, windowBackdropType);
    }

    private void OnClickLightTheme(object sender, RoutedEventArgs e)
    {
        var windowBackdropType = _settings.Application.Window.BackdropStyle.ToWindowBackdropType();
        _logger.LogTrace("Change theme to {Theme} and backdrop type {BackdropType}", ApplicationTheme.Light, windowBackdropType);
        ApplicationThemeManager.Apply(ApplicationTheme.Light, windowBackdropType);
    }

    private void OnLoaded(object _, RoutedEventArgs e)
    {
        SystemThemeWatcher.Watch(
            this,
            _settings.Application.Window.BackdropStyle.ToWindowBackdropType()
        );

        SetWindowPosition();
        ShowWindow();
    }

    private void OnLostKeyboardFocus(object _, RoutedEventArgs e)
    {
        /* This is how I handle a click on a result. I don't hide the window immediately;
         * I let the 'ExecuteCommand' run, which will handle hiding the window itself.
         */
        if (e is KeyboardFocusChangedEventArgs { NewFocus: ListViewItem }) return;

        ConditionalExecution.Execute(
            () => { },
            () => HideWindow()
        );
    }

    private void OnMouseDown(object _, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void OnMouseUp(object _, MouseButtonEventArgs e)
    {
        var coordinate = _databaseConfig.Current.Window.Position;

        if (e.ChangedButton != MouseButton.Left || this.IsAtPosition(coordinate)) return;

        _logger.LogInformation("Save new coordinate ({Top},{Left})", Top, Left);
        coordinate.Top = Top;
        coordinate.Left = Left;
        _databaseConfig.Save();
    }

    private void OnPreviewKeyDown(object _, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) HideWindow();
    }

    private void OnResultSelectionChanged(object _, SelectionChangedEventArgs e)
    {
        var current = Result.SelectedItem;
        Result.ScrollIntoView(current);
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        switch (menuItem.Tag)
        {
            case "showquery": ShowWindow(); break;

            case "settings":
                var view = _serviceProvider.GetService<SettingsView>()!;
                view.Show();
                view.Navigate<KeywordsView>();
                break;

            case "quit": Application.Current.Shutdown(); break;
        }
    }

    private void SetQuery(string suggestion)
    {
        QueryTextBox.Text = suggestion;
        QueryTextBox.Select(QueryTextBox.Text.Length, 0);
    }

    private void SetWindowPosition(Coordinate? coordinate = null)
    {
        coordinate ??= _databaseConfig!.Current.Window.Position.ToCoordinate();

        if (coordinate.IsEmpty)
            this.SetDefaultPosition();
        else
            this.SetPosition(coordinate);

        if (this.IsInScreen()) return;

        _logger.LogWarning("Window is out of screen {Coordinate}. Set it to default position at centre of the screen", this.ToCoordinate());
        this.SetDefaultPosition();
    }

    private void ShowWindow()
    {
        _logger.LogTrace("Current window is at {Coordinate}", this.ToCoordinate());

        ViewModel.RefreshSettings();
        _ = ViewModel.DisplayResultsIfAllowed();
        Visibility = Visibility.Visible;
        QueryTextBox.Focus();

        //https://stackoverflow.com/questions/3109080/focus-on-textbox-when-usercontrol-change-visibility
        Dispatcher.BeginInvoke((Action)delegate { Keyboard.Focus(QueryTextBox); });

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    public void OnShowWindow(object? _, HotkeyEventArgs? e)
    {
        ShowWindow();
        if (e is not null) e.Handled = true;
    }

    /// <summary>
    ///     Call this method to display the window the first time. It checks the settings and hides the window
    ///     immediately after showing it if the <c>ShowAtStartup</c> setting is set to <c>True</c>.
    ///     If <c>ShowAtStartup</c> is <c>False</c>, the window remains visible.
    /// </summary>
    public void ShowOnStartup()
    {
        if (ViewModel.ShowAtStartup) Show();
    }

    #endregion
}