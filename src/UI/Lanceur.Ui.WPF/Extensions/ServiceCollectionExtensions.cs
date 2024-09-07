using Windows.UI.Notifications;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUiServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IUiNotificationService, ToastUiNotificationService>();
        return serviceCollection;
    }
}