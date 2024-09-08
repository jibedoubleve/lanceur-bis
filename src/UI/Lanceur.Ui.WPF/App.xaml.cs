using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.WPF.Extensions;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wpf.Ui.Appearance;

namespace Lanceur.Ui.WPF;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    #region Fields

    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
                                                  .CreateDefaultBuilder()
                                                  .ConfigureServices((_, services) =>
                                                  {
                                                      services.AddViews()
                                                              .AddViewModels()
                                                              .AddServices()
                                                              .AddMapping()
                                                              .AddConfiguration()
                                                              .AddLoggers();
                                                  })
                                                  .Build();

    #endregion Fields

    #region Constructors

    public App() => DispatcherUnhandledException += OnDispatcherUnhandledException;

    #endregion Constructors

    #region Methods

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Host.Services.GetRequiredService<ILogger<App>>();
        var notify = Host.Services.GetRequiredService<IUiNotificationService>();

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
        Host.Services
            .GetRequiredService<MainView>()
            .Show();
        Host.Services.GetRequiredService<ILogger<App>>()!
            .LogInformation("Application started...");
    }

    #endregion Methods
}