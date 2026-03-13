using Lanceur.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Ui.Core.Extensions;

public static class ConfigurationRegistrationExtension
{
    #region Methods

    public static IServiceCollection AddConfigurationSections(this IServiceCollection serviceCollection)
        => serviceCollection.AddSettingsProviders()
                            .AddSingleton(typeof(IWriteableSection<>), typeof(Section<>))
                            .AddSingleton(typeof(ISection<>), typeof(ForwardingSection<>));

    #endregion
}