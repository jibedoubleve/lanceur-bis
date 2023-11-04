using Lanceur.Controls;
using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.Infra.Plugins;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;
using Lanceur.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using Splat;
using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Lanceur.Ui;

namespace Lanceur;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
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

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var notify = Locator.Current.GetService<IUserNotification>();
        notify.Error(e.Exception.Message, e.Exception);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ToastNotificationManagerCompat.Uninstall();
        SingleInstance.ReleaseMutex();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var log = Locator.Current.GetService<IAppLoggerFactory>().GetLogger<App>();
        _notifyIcon ??= new();

        Locator.Current.GetService<IImageCache>().LoadCache();

        ThemeManager.Current.SetTheme();

        if (!SingleInstance.WaitOne())
        {
            var notify = Locator.Current.GetService<IUserNotification>();
            notify.Warning("An instance of Lanceur is already running.");
            Environment.Exit(0);
        }

        var installer = Locator.Current.GetService<IPluginInstaller>();
        if (await installer.HasMaintenanceAsync())
        {
            var errors = await installer.SubscribeForInstallAsync();
            if (!errors.IsNullOrEmpty())
            {
                log.Error($"Error occured when installing plugins on startup: {errors}");
            }
        }

        File.Delete(Locations.MaintenanceLogBookPath);
    }

    #endregion Methods
}