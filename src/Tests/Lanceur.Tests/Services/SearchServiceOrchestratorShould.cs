using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Helpers;
using Lanceur.Ui.WPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class SearchServiceOrchestratorShould
{
    #region Methods

    [Theory]
    [InlineData(".")]
    [InlineData("")]
    [InlineData("m")]
    public void ConvertBackDotAsValueNotOperator(string input)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.ConvertBack(input, null!, null, null!)
                 .ShouldBe(input);
    }

    [Theory]
    [InlineData(@"^\s{0,}\..*", ".")]
    [InlineData(@"^\s{0,}.*", "")]
    [InlineData(@"^\s{0,}m.*", "m")]
    public void ConvertDotAsValueNotOperator(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.Convert(input, null!, null, null!)
                 .ShouldBe(output);
    }

    [Theory]
    [InlineData("m", "^\\s{0,}\\..*", false)]
    [InlineData(".", "^\\s{0,}\\..*", true)]
    [InlineData("m", "^\\s{0,}m.*", true)]
    [InlineData("mm", "^\\s{0,}m.*", true)]
    [InlineData("    mm", "^\\s{0,}m.*", true)]
    [InlineData("dm", "^\\s{0,}m.*", false)]
    [InlineData("/", "^\\s{0,}/.*", true)]
    //--
    [InlineData(".somest", "^\\s{0,}\\..*", true)]
    public void SelectExpectedCmdlines(string cmd, string regex, bool expected)
    {
        // arrange
        var sp = new ServiceCollection()
                 .AddLogging(builder => builder.AddXUnit())
                 .AddMockConfigurationSections()
                 .BuildServiceProvider();
        var storeService = Substitute.For<IStoreService>();
        storeService.StoreOrchestration.Returns(new StoreOrchestrationFactory().Exclusive(regex));

        var orchestrator = new SearchServiceOrchestrator(
            sp.GetSection<StoreSection>()!
        );

        // act
        orchestrator.IsAlive(storeService, Cmdline.Parse(cmd))
                    .ShouldBe(expected);
    }

    /// <summary>
    ///     Reproduces bug #1361: when <see cref="SteamGameStore" /> has its feature flag disabled
    ///     but a shortcut override exists in DB settings, <see cref="SearchServiceOrchestrator.IsAlive" />
    ///     incorrectly uses the override pattern instead of respecting the <c>AlwaysInactive</c>
    ///     constraint returned by <see cref="SteamGameStore.StoreOrchestration" />.
    /// </summary>
    [Fact]
    public void When_store_is_always_inactive_but_shortcut_override_exists_in_settings_Then_store_should_be_inactive()
    {
        // arrange
        var featureFlags = Substitute.For<IFeatureFlagService>();
        featureFlags.IsEnabled(Features.SteamIntegration).Returns(false);   // feature flag OFF

        var store = new SteamGameStore(
            new StoreOrchestrationFactory(),
            new StaticSection(new StoreSection()),
            Substitute.For<ISteamLibraryService>(),
            Substitute.For<IAliasManagementService>(),
            featureFlags
        );

        // A shortcut override for SteamGameStore is present in settings (e.g. saved earlier by the user)
        var section = new StaticSection(new StoreSection
        {
            StoreShortcuts =
            [
                new StoreShortcut
                {
                    StoreType     = store.GetType().ToString(),
                    AliasOverride = @"^\s{0,}&.*"   // matches "&steam", "&portal", etc.
                }
            ]
        });

        var orchestrator = new SearchServiceOrchestrator(section);

        // act — query starts with "&", which matches the override pattern
        var result = orchestrator.IsAlive(store, Cmdline.Parse("&steam"));

        // assert — AlwaysInactive must win: the disabled feature flag takes precedence over any stored shortcut
        result.ShouldBeFalse();
    }

    // Wraps a StoreSection value without any persistence logic
    private sealed class StaticSection(StoreSection value) : ISection<StoreSection>
    {
        public StoreSection Value => value;
        public void Reload() { }
    }

    #endregion
}
