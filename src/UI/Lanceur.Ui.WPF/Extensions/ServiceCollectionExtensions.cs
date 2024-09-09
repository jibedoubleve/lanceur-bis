using Lanceur.Core.Services;
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
        return serviceCollection.AddTransient<IUiNotificationService, ToastUiNotificationService>()
                                .AddSingleton<IPageService, PageService>()
                                .AddSingleton<MainView>()
                                .AddTransient<SettingsView>()
                                .AddTransient<DoubloonsView>()
                                .AddTransient<EmptyKeywords>()
                                .AddTransient<HistoryView>()
                                .AddTransient<KeywordsView>()
                                .AddTransient<MostUsedView>()
                                .AddTransient<PluginsView>()
                                .AddTransient<TrendsView>()
                                .AddTransient<ApplicationSettingsView>();
    }
}