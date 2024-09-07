using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.Helpers;
using Lanceur.Ui.WPF.Tools;
using Microsoft.Extensions.Logging;
using NHotkey;
using NHotkey.Wpf;

namespace Lanceur.Ui.WPF.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainView
{
    #region Fields

    private readonly ILogger<MainView> _logger;
    private readonly IAppConfigRepository? _settings;

    #endregion

    #region Constructors

    public MainView()
    {
        InitializeComponent();
        _settings = Ioc.Default.GetService<IAppConfigRepository>() ?? throw new ArgumentNullException(nameof(_settings));
        _logger = Ioc.Default.GetService<ILogger<MainView>>() ?? throw new ArgumentNullException(nameof(_logger));
        DataContext = Ioc.Default.GetService<MainViewModel>() ?? throw new ArgumentNullException(nameof(DataContext));

        var msgr = WeakReferenceMessenger.Default;
        msgr.Register<KeepAliveRequest>(
            this,
            (_, m) =>
            {
                if (m.Value)
                    ShowWindow();
                else
                    HideWindow();
            }
        );
        msgr.Register<ChangeCoordinateRequest>(this, (_, m) => SetWindowPosition(m.Value));
        msgr.Register<SetQueryRequest>(this, (_, m) => SetQuery(m.Value));
    }

    #endregion

    #region Properties

    private MainViewModel ViewModel => DataContext as MainViewModel ?? throw new ArgumentNullException($"DataContext should be of type {typeof(MainViewModel)} and not be <NULL>");

    #endregion

    #region Methods

    private void HideWindow()
    {
        QueryTextBox.SelectAll();
        Hide();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var hk = _settings!.Current.HotKey;
        SetGlobalShortcut((Key)hk.Key, (ModifierKeys)hk.ModifierKey);
        SetWindowPosition();

        ShowWindow();
    }

    private void OnLostKeyboardFocus(object sender, RoutedEventArgs e) { HideWindow(); }

    private void OnMouseDown(object _, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void OnMouseUp(object _, MouseButtonEventArgs e)
    {
        var coordinate = _settings!.Current.Window.Position;

        if (e.ChangedButton != MouseButton.Left || this.IsAtPosition(coordinate)) return;

        _logger.LogInformation("Save new coordinate ({Top},{Left})", Top, Left);
        coordinate.Top = Top;
        coordinate.Left = Left;
        _settings.Save();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) HideWindow();
    }

    private void OnSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var current = ResultListView.SelectedItem;
        ResultListView.ScrollIntoView(current);
    }

    private void OnShowWindow(object? sender, HotkeyEventArgs? e)
    {
        ShowWindow();
        if (e is not null) e.Handled = true;
    }

    private void SetGlobalShortcut(Key key, ModifierKeys modifier)
    {
        try { HotkeyManager.Current.AddOrReplace("OnShowWindow", key, modifier, OnShowWindow); }
        catch (HotkeyAlreadyRegisteredException ex)
        {
            //Default values
            _logger.LogWarning(ex, "Impossible to set shortcut. (Key: {Key}, Modifier: {Modifier})", key, modifier);
            SetGlobalShortcut(Key.R, ModifierKeys.Shift | ModifierKeys.Windows);
        }
    }

    private void SetQuery(string suggestion)
    {
        QueryTextBox.Text = suggestion;
        QueryTextBox.Select(QueryTextBox.Text.Length, 0);
    }

    private void SetWindowPosition(Coordinate? coordinate = null)
    {
        coordinate ??= _settings!.Current.Window.Position.ToCoordinate();

        if (coordinate.IsEmpty)
            this.SetDefaultPosition();
        else
            this.SetPosition(coordinate);

        if (this.IsOutOfScreen())
        {
            _logger.LogWarning("Window is out of screen {Coordinate}. Set it to default position at centre of the screen", this.ToCoordinate());
            this.SetDefaultPosition();
        }
    }

    private void ShowWindow()
    {
        Visibility = Visibility.Visible;
        QueryTextBox.Focus();

        //https://stackoverflow.com/questions/3109080/focus-on-textbox-when-usercontrol-change-visibility
        Dispatcher.BeginInvoke((Action)delegate { Keyboard.Focus(QueryTextBox); });

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    #endregion
}