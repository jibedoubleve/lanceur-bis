using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Stores;

public sealed class SteamGameStoreTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public SteamGameStoreTest(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    private SteamGameStore GetStore(
        ISteamLibraryService steamService,
        IAliasManagementService? managementService = null,
        string? shortcutOverride = null)
    {
        managementService ??= Substitute.For<IAliasManagementService>();
        managementService.HydrateSteamGameUsage(Arg.Any<IEnumerable<AliasQueryResult>>())
                         .Returns(x => x.Arg<IEnumerable<AliasQueryResult>>());

        var storeSection = new StoreSection();
        if (shortcutOverride is not null)
        {
            storeSection.StoreShortcuts =
            [
                new StoreShortcut
                {
                    StoreType    = typeof(SteamGameStore).FullName,
                    AliasOverride = shortcutOverride
                }
            ];
        }

        var sp = new ServiceCollection()
                 .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                 .AddSingleton(steamService)
                 .AddSingleton<SteamGameStore>()
                 .AddSingleton(managementService)
                 .AddMockSingleton<ISection<StoreSection>>((_, i) => {
                     i.Value.Returns(storeSection);
                     return i;
                 })
                 .AddMockSingleton<IFeatureFlagService>((_, i) => {
                     i.IsEnabled(Arg.Any<string>()).Returns(true);
                     return i;
                 })
                 .AddLoggingForTests(_output)
                 .BuildServiceProvider();
        return sp.GetRequiredService<SteamGameStore>();
    }

    private static ISteamLibraryService SteamServiceWith(params SteamGame[] games)
    {
        var service = Substitute.For<ISteamLibraryService>();
        service.GetGames().Returns(games);
        return service;
    }

    [Fact]
    public void When_canPrune_with_contains_semantics_Then_non_prefix_substring_refinement_is_recognised()
    {
        // ARRANGE
        var games = new SteamGame[]
        {
            new(440, "Half-Life 2"), // contains "li" AND "half-li" → kept after both searches
            new(100, "Real Life Simulator"), // contains "li" but NOT "half-li" → pruned at step 2
            new(200, "Portal") // contains "al" but NOT "li" → excluded from step 1
        };
        var store = GetStore(SteamServiceWith(games), Substitute.For<IAliasManagementService>());

        var previousQuery = Cmdline.Parse("& li");
        var refinedQuery = Cmdline.Parse("& half-li");

        // "half-li" contains "li" as a substring → pruning is valid for Contains semantics
        // But CanPruneResult uses StartsWith: "half-li".StartsWith("li") == false → BUG
        store.CanPruneResult(previousQuery, refinedQuery)
             .ShouldBeTrue();
    }

    [Fact]
    public void When_previous_query_has_no_parameter_Then_CanPruneResult_allows_refinement()
        => GetStore(SteamServiceWith()).CanPruneResult(Cmdline.Parse("&"), Cmdline.Parse("& a"))
                                       .ShouldBeTrue();

    [Fact]
    public void When_shortcut_is_overridden_without_capture_group_Then_CanPruneResult_is_false_for_filtered_query()
    {
        // ARRANGE — simulate a shortcut stored by the buggy ConvertBack (no capture group)
        const string storedShortcutWithoutCaptureGroup = @"^\s{0,}\&.*";
        var store = GetStore(SteamServiceWith(), shortcutOverride: storedShortcutWithoutCaptureGroup);

        var previous = Cmdline.Parse("& half");
        var current  = Cmdline.Parse("& half-life");

        // ACT & ASSERT — "half-life" contains "half" → pruning should be allowed.
        // With the bug, IsUnfiltered("& half") incorrectly returns true because Groups[1] is always
        // empty when there is no capture group, so CanPruneResult returns false.
        store.CanPruneResult(previous, current).ShouldBeTrue();
    }

    [Fact]
    public void When_shortcut_is_overridden_with_capture_group_Then_CanPruneResult_allows_pruning()
    {
        // ARRANGE — shortcut stored by the fixed ConvertBack (with capture group)
        const string storedShortcutWithCaptureGroup = @"^\s{0,}\&(.*)";
        var store = GetStore(SteamServiceWith(), shortcutOverride: storedShortcutWithCaptureGroup);

        var previous = Cmdline.Parse("& half");
        var current  = Cmdline.Parse("& half-life");

        // ACT & ASSERT — pruning must be allowed: "half-life" is a refinement of "half"
        store.CanPruneResult(previous, current).ShouldBeTrue();
    }

    [Fact]
    public void When_search_Then_hydrate_usage_is_called()
    {
        // ARRANGE
        var managementService = Substitute.For<IAliasManagementService>();
        var games = new SteamGame[] { new(440, "Half-Life 2") };
        var store = GetStore(SteamServiceWith(games), managementService);

        // ACT
        store.Search(Cmdline.Parse("&"));

        // ASSERT
        managementService.Received(1).HydrateSteamGameUsage(Arg.Any<IEnumerable<AliasQueryResult>>());
    }

    [Theory]
    [InlineData("&", 3)] // No parameter → return all
    [InlineData("&half", 1)] // Should return "Half-Life 2"
    [InlineData("&HALF", 1)] // Case-insensitive
    [InlineData("&zzz", 0)] // No result
    public void When_search_Then_results_are_filtered_by_parameter(string cmd, int expected)
    {
        // ARRANGE
        var games = new SteamGame[]
        {
            new(440, "Half-Life 2"),
            new(730, "Counter-Strike 2"),
            new(570, "Dota 2")
        };
        var store = GetStore(SteamServiceWith(games), Substitute.For<IAliasManagementService>());

        // ACT
        var results = store.Search(Cmdline.Parse(cmd)).ToArray();

        // ASSERT
        results.Length.ShouldBe(expected);
    }

    [Fact]
    public void When_search_Then_results_contain_steam_urls_as_filename()
    {
        // ARRANGE
        var games = new SteamGame[] { new(440, "Half-Life 2") };
        var store = GetStore(SteamServiceWith(games), Substitute.For<IAliasManagementService>());

        // ACT
        var results = store.Search(Cmdline.Parse("& half")).OfType<AliasQueryResult>().ToArray();

        // ASSERT
        results.ShouldSatisfyAllConditions(
            r => r.Length.ShouldBe(1),
            r => r[0].FileName.ShouldBe("steam://run/440")
        );
    }

    #endregion
}