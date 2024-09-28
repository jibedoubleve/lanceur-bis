using Lanceur.Core.Services;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Lanceur.Ui.WPF.Views;
using Lanceur.Ui.WPF.Views.Pages;
using Wpf.Ui;

namespace Lanceur.Ui.WPF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViews(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<MainView>()
                                .AddSingleton<SettingsView>()
                                .AddSingleton<DoubloonsView>()
                                .AddSingleton<EmptyKeywordsView>()
                                .AddSingleton<HistoryView>()
                                .AddSingleton<KeywordsView>()
                                .AddSingleton<MostUsedView>()
                                .AddSingleton<PluginsView>()
                                .AddSingleton<TrendsView>()
                                .AddSingleton<CodeEditorView>()
                                .AddSingleton<ApplicationSettingsView>();
    }

    public static IServiceCollection AddWpfServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IUiNotificationService, ToastUiNotificationService>()
                         .AddSingleton<IPageService, PageService>()
                         .AddSingleton<IContentDialogService, ContentDialogService>()
                         .AddSingleton<IUiUserInteractionService, UserInteractionService>()
                         .AddSingleton<ISnackbarService, SnackbarService>()
                         .AddSingleton<INotificationService, NotificationService>();
        
        return serviceCollection;
    }
}