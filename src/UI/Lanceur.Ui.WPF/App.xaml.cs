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

    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddViewModels()
                    .AddServices()
                    .AddViews()
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
        var logger = _host.Services.GetRequiredService<ILogger<App>>();
        var notify = _host.Services.GetRequiredService<IUiNotificationService>();

        logger?.LogCritical(e.Exception, "Fatal error: {Message}", e.Exception.Message);
        notify?.Error(e.Exception.Message);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.Services.GetRequiredService<ILogger<App>>()!
           .LogInformation("Application has closed");
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Ioc.Default.ConfigureServices(_host.Services);
        _host.Start();
        _host.Services
            .GetRequiredService<MainView>()
            .Show();
        _host.Services.GetRequiredService<ILogger<App>>()!
           .LogInformation("Application started...");
    }

    #endregion Methods
}