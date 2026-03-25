using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Configuration;

public sealed class SettingsProvidersTest : TestBase
{
    #region Constructors

    public SettingsProvidersTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private void WithConfiguration<T>(
        Action<Section<T>, SQLiteApplicationSettingsProvider> action)
        where T : class
    {
        var conn = BuildFreshDb();
        var scope = new DbSingleConnectionManager(conn);
        var logger = CreateLogger<SQLiteApplicationSettingsProvider>();
        var provider = new SQLiteApplicationSettingsProvider(scope, logger);
        var section = new Section<T>([provider]);
        action(section, provider);
    }

    /// <summary>
    ///     Test for issue #1334
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

    [Fact]
    public void When_provider_reloads_between_value_access_and_save_Then_modification_is_persisted()
        => WithConfiguration<WindowSection>((section, provider) => {
            // arrange
            _ = section.Value; // trigger initial load and prime the cache

            // act
            provider.Load(); // simulate a reload triggered by another component — the bug trigger
            section.Value.NotificationDisplayDuration = 99;
            section.Save();

            provider.Load(); // reload from DB to verify what was actually saved

            // assert
            provider.Value.Window.NotificationDisplayDuration.ShouldBe(99);
        });

    [Fact]
    public void When_section_is_modified_and_saved_Then_value_persists_after_reload()
        => WithConfiguration<WindowSection>((section, provider) => {
            // act
            section.Value.NotificationDisplayDuration = 99;
            section.Save();
            provider.Load();

            // assert
            provider.Value.Window.NotificationDisplayDuration.ShouldBe(99);
        });

    [Fact]
    public void When_store_shortcuts_are_saved_and_reloaded_Then_shortcuts_are_replaced_not_merged()
        => WithConfiguration<StoreSection>((section, provider) => {
            // arrange
            section.Value.StoreShortcuts = [new StoreShortcut { StoreType = "TypeA", AliasOverride = "a" }];
            section.Save();

            // act — reload twice to detect merge vs replace behaviour
            provider.Load();
            provider.Load();

            // assert — should still have exactly 1 shortcut, not 2 or 3
            provider.Value.Stores.StoreShortcuts.Count().ShouldBe(1);
        });

    [Fact]
    public void When_store_shortcuts_are_updated_and_reloaded_Then_old_shortcuts_are_not_kept()
        => WithConfiguration<StoreSection>((section, provider) => {
            // first save with 2 shortcuts
            section.Value.StoreShortcuts =
            [
                new StoreShortcut { StoreType = "TypeA", AliasOverride = "a" },
                new StoreShortcut { StoreType = "TypeB", AliasOverride = "b" }
            ];
            section.Save();

            // second save with only 1 shortcut (replacing the previous 2)
            section.Value.StoreShortcuts = [new StoreShortcut { StoreType = "TypeC", AliasOverride = "c" }];
            section.Save();

            // act
            provider.Load();

            // assert — should have exactly 1 shortcut, TypeA and TypeB must be gone
            provider.Value.Stores.StoreShortcuts.ShouldSatisfyAllConditions(
                s => s.Single().StoreType.ShouldBe("TypeC"),
                s => s.Count().ShouldBe(1)
            );
        });

    #endregion
}