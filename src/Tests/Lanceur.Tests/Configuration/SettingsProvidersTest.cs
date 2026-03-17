using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;
using Lanceur.Tests.Tools;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Configuration;

public class SettingsProvidersTest : TestBase
{
    #region Constructors

    public SettingsProvidersTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    /// <summary>
    /// Test for issue #1334
    /// </summary>
    [Fact]
    public void When_AddSettingsProviders_is_called_Then_generic_providers_are_registered()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.AddSettingsProviders();

        // assert
        services.Any(d => d.ServiceType == typeof(ISettingsProvider<ApplicationSettings>))
                .ShouldBeTrue("ISettingsProvider<ApplicationSettings> should be registered");

        services.Any(d => d.ServiceType == typeof(ISettingsProvider<InfrastructureSettings>))
                .ShouldBeTrue("ISettingsProvider<InfrastructureSettings> should be registered");
    }

    #endregion
}
