using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Configuration.Sections.Infrastructure;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Stubs;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Configuration;

public sealed class SectionTest : TestBase
{
    #region Constructors

    public SectionTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private SQLiteApplicationSettingsProvider CreateProvider()
    {
        var conn = BuildFreshDb();
        var scope = new DbSingleConnectionManager(conn);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();
        return new SQLiteApplicationSettingsProvider(scope, logger);
    }

    [Fact]
    public void When_provider_reloads_after_value_is_cached_Then_modification_is_visible_in_provider()
    {
        // arrange
        var provider = CreateProvider();
        var section = new Section<WindowSection>([provider]);
        _ = section.Value; // prime the cache

        // act — provider.Load() replaces _current; section._value becomes detached
        provider.Load();
        section.Value.NotificationDisplayDuration = 99;

        // assert — the modification must be reachable through the provider
        provider.Value.Window.NotificationDisplayDuration.ShouldBe(99);
    }

    [Fact]
    public void When_provider_reloads_Then_all_section_references_remain_consistent()
    {
        // arrange
        var provider = CreateProvider();
        var windowSection = new Section<WindowSection>([provider]);
        var searchSection = new Section<SearchBoxSection>([provider]);
        _ = windowSection.Value;
        _ = searchSection.Value;

        // act
        provider.Load();

        // assert — all sections must point to the new provider state
        ReferenceEquals(windowSection.Value, provider.Value.Window).ShouldBeTrue();
        ReferenceEquals(searchSection.Value, provider.Value.SearchBox).ShouldBeTrue();
    }

    [Fact]
    public void When_provider_reloads_Then_section_value_shares_reference_with_provider()
    {
        // arrange
        var provider = CreateProvider();
        var section = new Section<WindowSection>([provider]);
        _ = section.Value; // prime the cache

        // act
        provider.Load(); // replaces _current with a new deserialized instance

        // assert — section.Value must point to the new provider state, not the detached one
        ReferenceEquals(section.Value, provider.Value.Window).ShouldBeTrue();
    }

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

        // ISettingsProvider<T>.Value uses "new" (hiding, not override), so NSubstitute
        // configures it as a separate member from ISettingsProvider.Value.
        // Casting to the base interface ensures we stub the property that Section<T> actually reads.
        var testProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<TestSettingsWithTwiceSameSection>>();
        var infraProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        testProvider.Value.Returns(testSettings);
        infraProvider.Value.Returns(infraSettings);

        var section = new Section<DatabaseSection>([
            testProvider,
            infraProvider
        ]);

        // act & assert
        Assert.Throws<InvalidOperationException>(() => section.Value);
    }

    [Fact]
    public void When_section_reloads_Then_value_re_resolves_from_provider()
    {
        // arrange
        var provider = CreateProvider();
        var section = new Section<WindowSection>([provider]);
        _ = section.Value; // prime the cache

        // act — section.Reload() calls provider.Load() AND nullifies the cached _value
        section.Reload();

        // assert — next access re-resolves from the fresh provider state
        ReferenceEquals(section.Value, provider.Value.Window).ShouldBeTrue();
    }

    [Fact]
    public void When_section_type_is_not_found_Then_throws()
    {
        // arrange
        var appSettings = new ApplicationSettings();
        var infraSettings = new InfrastructureSettings();

        // ISettingsProvider<T>.Value uses "new" (hiding, not override), so NSubstitute
        // configures it as a separate member from ISettingsProvider.Value.
        // Casting to the base interface ensures we stub the property that Section<T> actually reads.
        var appProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<TestSettingsWithTwiceSameSection>>();
        var infraProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        appProvider.Value.Returns(appSettings);
        infraProvider.Value.Returns(infraSettings);

        var section = new Section<object>([
            appProvider,
            infraProvider
        ]);

        // act & assert
        Assert.Throws<InvalidOperationException>(() => section.Value);
    }

    [Fact]
    public void When_section_value_is_modified_Then_change_is_reflected_in_provider()
    {
        // arrange
        var provider = CreateProvider();
        var section = new Section<WindowSection>([provider]);

        // act
        section.Value.NotificationDisplayDuration = 99;

        // assert — Section<T>.Value and provider.Value.Window are the same reference
        provider.Value.Window.NotificationDisplayDuration.ShouldBe(99);
    }

    [Fact]
    public void When_using_memory_provider_Then_value_is_resolved()
    {
        // arrange
        var appSettings = new ApplicationSettings();
        var infraSettings = new InfrastructureSettings();

        // ISettingsProvider<T>.Value uses "new" (hiding, not override), so NSubstitute
        // configures it as a separate member from ISettingsProvider.Value.
        // Casting to the base interface ensures we stub the property that Section<T> actually reads.
        var appProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<TestSettingsWithTwiceSameSection>>();
        var infraProvider = (ISettingsProvider)Substitute.For<ISettingsProvider<InfrastructureSettings>>();

        appProvider.Value.Returns(appSettings);
        infraProvider.Value.Returns(infraSettings);

        var section = new Section<SearchBoxSection>([
            appProvider,
            infraProvider
        ]);

        // act & assert
        section.Value.ShouldNotBeNull();
    }

    #endregion
}