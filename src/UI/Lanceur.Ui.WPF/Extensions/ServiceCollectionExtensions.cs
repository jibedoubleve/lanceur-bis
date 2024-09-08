using Lanceur.Core.Services;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Lanceur.Ui.WPF.Views;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViews(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddTransient<IUiNotificationService, ToastUiNotificationService>()
                                .AddSingleton<MainView>()
                                .AddTransient<SettingsView>();
    }
}