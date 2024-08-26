using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Ioc.Default.ConfigureServices(
            new ServiceCollection().AddViewModels()
                                   .AddServices()
                                   .AddMapping()
                                   .AddConfiguration()
                                   .AddLoggers()
                                   .BuildServiceProvider()
        );
    }
}