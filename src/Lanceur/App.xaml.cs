using Lanceur.Controls;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.Infra.Plugins;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;
using Lanceur.Utils;
using Lanceur.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using Splat;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Lanceur.Infra.Win32.Thumbnails;

namespace Lanceur;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    #region Fields

    private NotifyIconAdapter _notifyIcon;

    #endregion Fields

    #region Constructors

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        Bootstrapper.Initialise();
    }

    #endregion Constructors

    #region Methods

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Locator.Current.GetLogger<App>();
        try
        {
            var notify = Locator.Current.GetService<IUserNotification>();
            notify.Error(e.Exception.Message, e.Exception);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application crashed. See error for further information");

            if (Current?.MainWindow is not null) { MessageBox.Show(Current.MainWindow, $"A fatal error occured: {ex.Message}"); }
            else { MessageBox.Show($"A fatal error occured: {ex.Message}"); }
        }
        finally { Bootstrapper.TearDown(); }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Bootstrapper.TearDown();

        ToastNotificationManagerCompat.Uninstall();
        SingleInstance.ReleaseMutex();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var logger = Locator.Current.GetLogger<App>();
        _notifyIcon ??= new();

        // THEME
        ThemeManager.Current.SetTheme();

        // SINGLE INSTANCE
        if (!SingleInstance.WaitOne())
        {
            var notify = Locator.Current.GetService<IUserNotification>();
            notify.Warning("An instance of Lanceur is already running.");
            Environment.Exit(0);
        }

        // PLUGINS
        var installer = Locator.Current.GetService<IPluginInstaller>();
        if (await installer.HasMaintenanceAsync())
        {
            var errors = await installer.SubscribeForInstallAsync();
            if (!errors.IsNullOrEmpty()) logger.LogError("Error occured when installing plugins on startup: {Errors}", errors);
        }

        File.Delete(Locations.MaintenanceLogBookPath);
        
        // PRELOAD
        _ = Locator.Current.GetService<ISearchService>()
                   ?.GetAllAsync();
    }

    #endregion Methods
}