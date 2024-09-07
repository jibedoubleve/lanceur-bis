using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Extensions;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.WPF.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public App() => DispatcherUnhandledException += OnDispatcherUnhandledException;

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Ioc.Default.GetService<ILogger<App>>();
        var notify = Ioc.Default.GetService<IUiNotificationService>();
        
        logger?.LogCritical(e.Exception, "Fatal error: {Message}", e.Exception.Message);
        notify?.Error(e.Exception.Message);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection().AddViewModels()
                                   .AddServices()
                                   .AddUiServices()
                                   .AddMapping()
                                   .AddConfiguration()
                                   .AddLoggers()
                                   .BuildServiceProvider()
        );
        Ioc.Default.GetService<ILogger<App>>()!
           .LogInformation("Application started...");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Ioc.Default.GetService<ILogger<App>>()!
           .LogInformation("Application has closed");
    }
}