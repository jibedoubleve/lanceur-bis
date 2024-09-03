using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Repositories.Config;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels;
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

        WeakReferenceMessenger.Default.Register<KeepAliveRequest>(
            this,
            (_, m) =>
            {
                if (m.Value)
                    ShowWindow();
                else
                    HideWindow();
            }
        );
    }

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

    private void OnMouseDown(object _, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void OnMouseUp(object _, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            _logger.LogInformation("Save new coordinate ({Top},{Left})", Top, Left);
            _settings!.Current.Window.Position.Top = Top;
            _settings!.Current.Window.Position.Left = Left;
            _settings.Save();
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            HideWindow();
            return;
        }

        var max = QueryResultControl.Items.Count - 1;
        var current = QueryResultControl.SelectedIndex;

        QueryResultControl.SelectedIndex = e.Key switch
        {
            Key.Down => current >= max ? 0 : current + 1,
            Key.Up   => current == 0 ? max : current - 1,
            _        => QueryResultControl.SelectedIndex
        };
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

    private void SetWindowPosition()
    {
        var top = _settings!.Current.Window.Position.Top;
        var left = _settings!.Current.Window.Position.Left;
        var coordinate = new Coordinate(top, left);

        if (coordinate.IsEmpty)
            this.SetDefaultPosition();
        else
            this.SetPosition(coordinate);

        if (this.IsOutOfScreen())
        {
            _logger.LogInformation("Window is out of screen. Set it to default position at centre of the screen");
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