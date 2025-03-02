using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Constants;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.WPF.Extensions;
using Lanceur.Ui.WPF.Services;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using Serilog;

namespace Lanceur.Ui.WPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    #region Fields

    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
                                                  .CreateDefaultBuilder()
                                                  .ConfigureServices(
                                                      (context, services) =>
                                                      {
                                                          services.AddTrackedMemoryCache()
                                                                  .Register("View", "Lanceur.Ui.WPF")
                                                                  .Register("ViewModel", "Lanceur.Ui.Core")
                                                                  .AddServices()
                                                                  .AddWpfServices()
                                                                  .AddMapping()
                                                                  .AddConfiguration()
                                                                  .AddDatabaseServices()
                                                                  .AddLoggers(context);
                                                      }
                                                  )
                                                  .ConfigureAppConfiguration(
                                                      (context, config) =>
                                                      {
                                                          if (context.HostingEnvironment.IsDevelopment()) config.AddUserSecrets<App>();
                                                      }
                                                  )
                                                  .Build();

    #endregion

    #region Constructors

    public App() => DispatcherUnhandledException += OnDispatcherUnhandledException;

    #endregion

    #region Methods

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Host.Services.GetRequiredService<ILogger<App>>();
        var notify = Host.Services.GetRequiredService<IUserGlobalNotificationService>();

        logger.LogCritical(e.Exception, "Fatal error: {Message}", e.Exception.Message);
        notify.Error(e.Exception.Message, e.Exception);
        Log.CloseAndFlush();
    }

    private static void RegisterToastNotifications()
    {
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            var arguments = ToastArguments.Parse(toastArgs.Argument);
            if (arguments.Count == 0) return;

            Action action = arguments["Type"] switch
            {
                ToastNotificationArguments.ClickShowError => () =>
                {
                    var view = Host.Services.GetService<ExceptionView>()!;
                    view.ExceptionMessage.Text = arguments["Message"];
                    view.ExceptionTrace.Text = arguments["StackTrace"];
                    view.Show();
                },
                ToastNotificationArguments.ClickShowLogs  => () => Process.Start("explorer.exe", Paths.LogRepository),
                _ => () => Log.Warning("The argument '{Argument}' is not supported in the toast arguments. Are you using a button that has not been configured yet?", toastArgs.Argument)
            };

            Current.Dispatcher.Invoke(
                delegate { action.Invoke(); }
            );
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Remove all persisting notifications...
        ToastNotificationManagerCompat.Uninstall();

        // Release mutex to allow app to start again...
        ConditionalExecution.Execute(
            () => { },
            SingleInstance.ReleaseMutex
        );
        
        Host.Services.GetRequiredService<ILogger<App>>()!
            .LogInformation("Application has closed");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Ioc.Default.ConfigureServices(Host.Services);
        Host.Start();
        RegisterToastNotifications();

        /* Checks whether database update is needed...
         */
        var cs = Ioc.Default.GetService<IConnectionString>()!;
        Ioc.Default.GetService<SQLiteUpdater>()!
           .Update(cs.ToString());

        /* Register HotKey to the application
         */
        var mainView = Host.Services.GetRequiredService<MainView>();
        var hotKeyService = Ioc.Default.GetService<IHotKeyService>()!;

        var hk = new Conditional<HotKeySection>(
            new((int)(ModifierKeys.Windows | ModifierKeys.Control), (int)Key.R),
            hotKeyService.HotKey
        );
        
        /* Only one instance allowed in prod...
         */
        ConditionalExecution.Execute(
            () => { },
            () =>
            {
                if (SingleInstance.WaitOne()) return;

                const string msg = "The application is already running.";
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0);
            }
        );

        var success = hotKeyService.RegisterHandler(mainView.OnShowWindow, hk);
        if (!success)
        {
            // Should be only useful in debug mode as Mutex should avoid this situation...
            var errorMessage = $"The shortcut '{hk.Value.ToStringHotKey()}' is already registered.";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
            return;
        }

        /* Now all preliminary stuff is done, let's start the application
         */
        if (mainView.ViewModel.ShowAtStartup) mainView.ShowOnStartup();

        Host.Services.GetRequiredService<ILogger<App>>()!
            .LogInformation("Application started");
    }

    #endregion
}