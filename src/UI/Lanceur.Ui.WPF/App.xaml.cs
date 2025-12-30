using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Constants;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Win32.Services;
using Lanceur.SharedKernel.IoC;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.WPF.Commands;
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
                                                  .ConfigureServices((context, services) =>
                                                      {
                                                          services.AddTrackedMemoryCache()
                                                                  .RegisterView("Lanceur.Ui.WPF")
                                                                  .RegisterViewModel("Lanceur.Ui.Core")
                                                                  .Register("Control", "Lanceur.Ui.WPF")
                                                                  .AddServices()
                                                                  .AddWpfServices()
                                                                  .AddStoreServices()
                                                                  .AddCommands()
                                                                  .AddConfiguration()
                                                                  .AddDatabaseServices()
                                                                  .AddLoggers(context, services.BuildServiceProvider());
                                                      }
                                                  )
                                                  .ConfigureAppConfiguration((context, config) =>
                                                      {
                                                          if (context.HostingEnvironment.IsDevelopment())
                                                              config.AddUserSecrets<App>();
                                                      }
                                                  )
                                                  .Build();

    #endregion

    #region Constructors

    public App() => DispatcherUnhandledException += OnDispatcherUnhandledException;

    #endregion

    #region Properties

    public static SynchronizationContext? UiContext { get; private set; }

    #endregion

    #region Methods

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Host.Services.GetRequiredService<ILogger<App>>();
        var notify = Host.Services.GetRequiredService<IUserGlobalNotificationService>();

        logger.LogCritical(e.Exception, "Fatal error: {Message}", e.Exception.Message);
        notify.Error(e.Exception.Message, e.Exception);
        Log.CloseAndFlush();
    }

    private static (bool Success, HotKeySection Hotkey) RegisterHandler(
        ILogger<App> logger,
        IHotKeyService hotKeyService,
        MainView mainView
    )
    {
        var view = Ioc.Default.GetRequiredService<HotkeyBoxView>();
        var dialogResult = view.ShowDialog();
        var hotkey = new HotKeySection((int)view.ViewModel.ModifierKeys, view.ViewModel.Key);
        if (!dialogResult.HasValue || !dialogResult.Value)
        {
            Current.Shutdown();
            return (true, hotkey);
        }

        logger.LogInformation(
            "Registering new HotKey {Modifiers} + {Key}",
            view.ViewModel.ModifierKeys,
            view.ViewModel.Key
        );

        return (
            hotKeyService.RegisterHandler(mainView.OnShowWindow, hotkey),
            hotkey
        );
    }

    private static void RegisterToastNotifications()
    {
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            var arguments = ToastArguments.Parse(toastArgs.Argument);
            if (arguments.Count == 0) return;

            Action action = arguments["Type"] switch
            {
                // ---- Show logs on error ----
                ToastNotificationArguments.ClickShowError => () =>
                {
                    var view = Host.Services.GetService<ExceptionView>()!;
                    view.ExceptionMessage.Text = arguments["Message"];
                    view.ExceptionTrace.Text = arguments["StackTrace"];
                    view.Show();
                },
                ToastNotificationArguments.ClickShowLogs  => () => Process.Start("explorer.exe", Paths.LogRepository),
                // ---- Restart application ----
                ToastNotificationArguments.ClickRestart => ()
                    => Host.Services.GetRequiredService<IAppRestartService>().Restart(),
                // ---- Visit Website ----
                ToastNotificationArguments.VisitWebsite => () => Process.Start("explorer.exe", Paths.ReleasesUrl),
                // ---- Skip current version ----
                ToastNotificationArguments.SkipVersion => () =>
                {
                    var settings = Host.Services.GetRequiredService<IConfigurationFacade>();
                    settings.Application.Github.SnoozeVersionCheck = true;
                    settings.Application.Github.LastCheckedVersion = new(arguments["Version"]);
                    settings.Save();
                },
                // ---- Navigate to Url ----
                ToastNotificationArguments.ClickNavigateIssue => () => Process.Start("explorer.exe", arguments["Url"]),
                // ---- Default  ----
                _ => () => Log.Warning(
                    "The argument {Argument} is not supported in the toast arguments. Are you using a button that has not been configured yet?",
                    toastArgs.Argument
                )
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
        var logger = Host.Services.GetRequiredService<ILogger<App>>();

        logger.LogInformation("=============== STARTUP ===============");

        using var measure = TimeMeter.Measure<App>(logger);

        Host.Start();
        RegisterToastNotifications();

        /* Only one instance allowed in prod...
         */
        ConditionalExecution.ExecuteOnRelease(() =>
            {
                if (SingleInstance.WaitOne()) return;

                const string msg = "The application is already running.";
                MessageBox.Show(
                    msg,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                Environment.Exit(0);
            }
        );
        
        /* Display application version in the logs
         */
        logger.LogVersion(Assembly.GetExecutingAssembly());

        /* Checks whether a database update is needed...
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

        var success = hotKeyService.RegisterHandler(mainView.OnShowWindow, hk);
        while (!success)
        {
            (success, var hotkey) = RegisterHandler(logger, hotKeyService, mainView);
            if (success) continue;

            // Should be only useful in debug mode as Mutex should avoid this situation...
            var errorMessage = $"The shortcut '{hotkey.ToStringHotKey()}' is already registered.";
            MessageBox.Show(
                errorMessage,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        /* Now all preliminary stuff is done, let's start the application
         */
        if (mainView.ViewModel.ShowAtStartup) mainView.ShowOnStartup();

        /* Check if new Version
         */
        var settings = Host.Services.GetRequiredService<IConfigurationFacade>()!;
        _ = Host.Services.GetRequiredService<IReleaseService>()
                .HasUpdateAsync()
                .ContinueWith(
                    context =>
                    {
                        if (!context.Result.HasUpdate || settings.Application.Github.SnoozeVersionCheck) return;

                        // A new version has been release, notify user...
                        Host.Services.GetService<UpdateNotification>()!
                            .Notify(context.Result.Version);
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion
                )
                .ContinueWith(
                    context => { logger.LogWarning(context.Exception, "En error occured while checking update."); },
                    TaskContinuationOptions.OnlyOnFaulted
                );

        base.OnStartup(e);
        UiContext = SynchronizationContext.Current!;
    }

    #endregion
}