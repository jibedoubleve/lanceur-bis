using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.Extensions;

public static class ConfigurationRegistrationExtension
{
    #region Methods

    public static IServiceCollection AddConfigurationSections(this IServiceCollection serviceCollection)
        => serviceCollection.AddSettingsProviders()
                            .AddSingleton(typeof(IWriteableSection<>), typeof(Section<>))
                            .AddSingleton(typeof(ISection<>), typeof(ForwardingSection<>))
                            .AddSingleton<ISection<DatabaseSection>>(_ =>
                                new Section<DatabaseSection>([
                                    SettingsProviderFactory.GetInfrastructureSettingsProvider()
                                ])
                            );

    #endregion
}