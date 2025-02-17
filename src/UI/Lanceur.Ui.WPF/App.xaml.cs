using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.SharedKernel.DI;
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

    private void RegisterToastNotifications()
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
                ToastNotificationArguments.ClickShowLogs  => () =>
                {
                    var path = Environment.ExpandEnvironmentVariables(@"%appdata%\probel\lanceur2\logs");
                    Process.Start("explorer.exe", path);
                },
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
        Ioc.Default.GetService<SQLiteUpdater>()!.Update(cs.ToString());

        /* Register HotKey to the application
         */
        var mainView = Host.Services.GetRequiredService<MainView>();
        var notification = Host.Services.GetRequiredService<IUserGlobalNotificationService>();
        var hotKeyService = Ioc.Default.GetService<IHotKeyService>()!;
        var isRegistered = hotKeyService.RegisterHandler(nameof(mainView.OnShowWindow), mainView.OnShowWindow);
        string hotKey;

        if (isRegistered) { hotKey = hotKeyService.GetHotkeyToString(); }
        else
        {
            var hk = new HotKeySection((int)(ModifierKeys.Windows | ModifierKeys.Control), (int)Key.R);
            hotKey = "Control, Windows - R";

            var success = hotKeyService.RegisterHandler(hotKey, mainView.OnShowWindow, hk);
            if (!success) throw new NotSupportedException($"The shortcut '{hotKey}' cannot be registered.");
        }

        /* Now all preliminary stuff is done, let's start the application
         */
        if (mainView.ViewModel.ShowAtStartup)
            mainView.ShowOnStartup();

        Host.Services.GetRequiredService<ILogger<App>>()!
            .LogInformation("Application started");
    }

    #endregion
}