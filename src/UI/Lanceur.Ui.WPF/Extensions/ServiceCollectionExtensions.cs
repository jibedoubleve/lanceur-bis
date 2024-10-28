using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Restart;
using Lanceur.SharedKernel.Utils;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Lanceur.Ui.WPF.Views;
using Lanceur.Ui.WPF.Views.Pages;
using Wpf.Ui;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWpfServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUserGlobalNotificationService, ToastUserNotificationService>()
                         .AddSingleton<IPageService, PageService>()
                         .AddSingleton<IContentDialogService, ContentDialogService>()
                         .AddSingleton<IUserInteractionService, UserUserInteractionService>()
                         .AddSingleton<ISnackbarService, SnackbarService>()
                         .AddSingleton<IUserNotificationService, UserNotificationService>();

        ConditionalExecution.Set(
            serviceCollection,
            onPrd => onPrd.AddSingleton<IAppRestart, DummyAppRestart>(),
            onDbg => onDbg.AddSingleton<IAppRestart, AppRestart>()
        );

        return serviceCollection;
    }
}