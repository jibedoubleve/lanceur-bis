using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Configuration.Sections.Infrastructure;
using Lanceur.Core.Services;
using Lanceur.Tests.Tools.Stubs;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Configuration;

public class SectionTest
{
    #region Methods

    [Fact]
    public void When_reload_configuration_Then_cache_is_invalidated()
    {
        // arrange
        var appProvider = Substitute.For<ISettingsProvider<ApplicationSettings>>();
        var infraProvider = Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        var section = new Section<DatabaseSection>([
            appProvider,
            infraProvider
        ]);

        // act
        section.Reload();

        // assert
        appProvider.Received(1).Load();
        infraProvider.Received(1).Load();
    }

    [Fact]
    public void When_section_is_registered_twice_Then_throws()
    {
        // arrange
        var testSettings = new TestSettingsWithTwiceSameSection();
        var infraSettings = new InfrastructureSettings();

        // ISettingsProvider<T>.Current uses "new" (hiding, not override), so NSubstitute                                                                                                                                                                                                                                                                                                                                                                                               
        // configures it as a separate member from ISettingsProvider.Current.                                                                                                                                                                                                                                                                                                                                                                                                           
        // Casting to the base interface ensures we stub the property that Section<T> actually reads.     
        var testProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<TestSettingsWithTwiceSameSection>>();
        var infraProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        testProvider.Current.Returns(testSettings);
        infraProvider.Current.Returns(infraSettings);

        var section = new Section<DatabaseSection>([
            testProvider,
            infraProvider
        ]);

        // act & assert
        Assert.Throws<InvalidOperationException>(() => section.Value);
    }

    [Fact]
    public void When_section_type_is_not_found_Then_throws()
    {
        // arrange
        var appSettings = new ApplicationSettings();
        var infraSettings = new InfrastructureSettings();

        // ISettingsProvider<T>.Current uses "new" (hiding, not override), so NSubstitute                                                                                                                                                                                                                                                                                                                                                                                               
        // configures it as a separate member from ISettingsProvider.Current.                                                                                                                                                                                                                                                                                                                                                                                                           
        // Casting to the base interface ensures we stub the property that Section<T> actually reads.     
        var appProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<TestSettingsWithTwiceSameSection>>();
        var infraProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        appProvider.Current.Returns(appSettings);
        infraProvider.Current.Returns(infraSettings);
        var section = new Section<object>([
            appProvider,
            infraProvider
        ]);

        // act & assert
        Assert.Throws<InvalidOperationException>(() => section.Value);
    }

    [Fact]
    public void When_using_memory_provider_Then_value_is_resolved()
    {
        // arrange
        var appSettings = new ApplicationSettings();
        var infraSettings = new InfrastructureSettings();

        // ISettingsProvider<T>.Current uses "new" (hiding, not override), so NSubstitute                                                                                                                                                                                                                                                                                                                                                                                               
        // configures it as a separate member from ISettingsProvider.Current.                                                                                                                                                                                                                                                                                                                                                                                                           
        // Casting to the base interface ensures we stub the property that Section<T> actually reads.     
        var appProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<TestSettingsWithTwiceSameSection>>();
        var infraProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        appProvider.Current.Returns(appSettings);
        infraProvider.Current.Returns(infraSettings);
        
        // arrange
        var section = new Section<SearchBoxSection>([
            appProvider,
            infraProvider
        ]);

        // act & assert
        section.Value.ShouldNotBeNull();
    }

    #endregion
}