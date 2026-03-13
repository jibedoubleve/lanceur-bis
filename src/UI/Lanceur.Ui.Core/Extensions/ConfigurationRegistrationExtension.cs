using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.Extensions;

public static class ConfigurationRegistrationExtension
{
    #region Methods

    public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection)
        => serviceCollection.AddTransient<IGithubService, GithubService>(); // TODO: move this where it belongs

    public static IServiceCollection AddConfigurationSections(this IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton(typeof(ISection<>), typeof(Section<>))
                            .AddSettingsProviders()
                            .AddSingleton<ISettingsProviderFacade, SettingsProviderFacade>()
                            .AddSingleton(typeof(IWriteableSection<>), typeof(Section<>));

    #endregion
}