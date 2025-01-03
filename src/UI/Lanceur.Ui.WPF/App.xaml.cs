using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.Extensions;
using Lanceur.SharedKernel.DI;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.WPF.Extensions;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                                                      (_, services) =>
                                                      {
                                                          services.Register("View", "Lanceur.Ui.WPF")
                                                                  .Register("ViewModel", "Lanceur.Ui.Core")
                                                                  .AddServices()
                                                                  .AddWpfServices()
                                                                  .AddMapping()
                                                                  .AddConfiguration()
                                                                  .AddDatabaseServices()
                                                                  .AddLoggers();
                                                      }
                                                  )
                                                  .Build();

    #endregion Fields

    #region Constructors

    public App() => DispatcherUnhandledException += OnDispatcherUnhandledException;

    #endregion Constructors

    #region Methods

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Host.Services.GetRequiredService<ILogger<App>>();
        var notify = Host.Services.GetRequiredService<IUserGlobalNotificationService>();

        logger.LogCritical(e.Exception, "Fatal error: {Message}", e.Exception.Message);
        notify.Error(e.Exception.Message);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Host.Services.GetRequiredService<ILogger<App>>()!
            .LogInformation("Application has closed");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Ioc.Default.ConfigureServices(Host.Services);
        Host.Start();

        /* Checks whether database update is needed...
         */
        var cs = Ioc.Default.GetService<IConnectionString>()!;
        Ioc.Default.GetService<SQLiteUpdater>()!
                   .Update(cs.ToString());
        
        /* Now the database is up to date, let's start the application
         */
        Host.Services
            .GetRequiredService<MainView>()
            .Show();
        Host.Services.GetRequiredService<ILogger<App>>()!
            .LogInformation("Application started");

    }

    #endregion Methods
}