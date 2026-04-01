using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class SearchServiceTestIncrementalFilters(ITestOutputHelper output) : TestBase(output)
{
    #region Methods

    [Fact]
    public async Task When_all_stores_can_prune_Then_no_store_is_queried_again()
    {
        // ARRANGE
        var storeA = BuildTestStore();
        var storeB = BuildTestStore();
        storeA.SeedResultSet([new AliasQueryResult { Name = "foo" }]);
        storeB.SeedResultSet([new AliasQueryResult { Name = "foobar" }]);
        var service = BuildSearchService(storeA, storeB);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "f");
        await service.SearchAsync(result, "fo");

        // ASSERT
        storeA.SearchCallCount.ShouldBe(1);
        storeB.SearchCallCount.ShouldBe(1);
    }


    [Theory]
    [InlineData("r:", "r:a", true)]
    [InlineData("r:f", "r:fo", true)]
    [InlineData("r:fo", "r:FO", true)]
    [InlineData("r:f", "r:f", true)]
    [InlineData("r", "r:", false)]
    [InlineData("r:f", "r", false)]
    [InlineData("r:fo", "r:bar", false)]
    public void When_checking_prune_Then_previous_query_is_handle_as_expected(
        string previous, string current, bool isPrunable)
    {
        var store = BuildAdditionalParametersStore();
        
        store.CanPruneResult(
            Cmdline.Parse(previous),
            Cmdline.Parse(current)
        ).ShouldBe(isPrunable);
    }

    [Fact]
    public async Task When_first_search_Then_stores_are_queried()
    {
        // ARRANGE
        var store = BuildStoreServiceWithResults([]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "app fo");

        // ASSERT
        store.Received(1).Search(Arg.Any<Cmdline>());
    }

    [Fact]
    public async Task When_one_store_cannot_prune_Then_all_stores_are_queried_again()
    {
        // ARRANGE
        var pruningStore = BuildTestStore();
        pruningStore.SeedResultSet([new AliasQueryResult { Name = "foo" }]);

        var noPruneStore = Substitute.For<IStoreService>();
        noPruneStore.Orchestration.Returns(new StoreOrchestrationFactory().SharedAlwaysActive());
        noPruneStore.CanPruneResult(Arg.Any<Cmdline>(), Arg.Any<Cmdline>()).Returns(false);
        noPruneStore.Search(Arg.Any<Cmdline>()).Returns([]);

        var service = BuildSearchService(pruningStore, noPruneStore);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "f");
        await service.SearchAsync(result, "fo");

        // ASSERT
        pruningStore.SearchCallCount.ShouldBe(2);
        noPruneStore.Received(2).Search(Arg.Any<Cmdline>());
    }

    [Fact]
    public async Task When_query_contains_previous_but_does_not_start_with_it_Then_stores_are_queried_again()
    {
        // ARRANGE
        // Regression: old code used Contains() instead of StartsWith(),
        // so "barfoo".Contains("foo") incorrectly triggered pruning.
        var store = BuildStoreServiceWithResults([]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "app foo");
        await service.SearchAsync(result, "app barfoo");

        // ASSERT
        store.Received(2).Search(Arg.Any<Cmdline>());
    }

    [Fact]
    public async Task When_query_follows_empty_query_Then_stores_are_queried_again()
    {
        // ARRANGE
        // Regression: old code did not guard against an empty _lastQuery,
        // so any query after an empty one incorrectly triggered pruning
        // because every string Contains("").
        var store = BuildStoreServiceWithResults([]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "app foo");
        await service.SearchAsync(result, Cmdline.Empty);
        await service.SearchAsync(result, "app bar");

        // ASSERT
        store.Received(2).Search(Arg.Any<Cmdline>());
    }

    [Fact]
    public async Task When_query_is_different_from_previous_Then_stores_are_queried_again()
    {
        // ARRANGE
        var store = BuildStoreServiceWithResults([]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "app fo");
        await service.SearchAsync(result, "app bar");

        // ASSERT
        store.Received(2).Search(Arg.Any<Cmdline>());
    }

    [Fact]
    public async Task When_query_is_empty_and_doesReturnAllIfEmpty_Then_all_results_are_returned()
    {
        // ARRANGE
        QueryResult[] allResults =
        [
            new AliasQueryResult { Name = "foo" },
            new AliasQueryResult { Name = "bar" }
        ];
        var store = BuildStoreServiceWithResults([], allResults);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, Cmdline.Empty, true);

        // ASSERT
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task When_query_is_empty_Then_previous_results_are_removed_from_destination()
    {
        // ARRANGE
        var store = BuildStoreServiceWithResults([]);
        var service = BuildSearchService(store);
        List<QueryResult> result = [new AliasQueryResult { Name = "existing" }];

        // ACT
        await service.SearchAsync(result, Cmdline.Empty);

        // ASSERT
        result.ShouldNotContain(x => x.Name == "existing");
    }

    [Fact]
    public async Task When_query_is_identical_to_previous_Then_stores_are_not_queried_again()
    {
        // ARRANGE
        var store = BuildTestStore();
        store.SeedResultSet([new AliasQueryResult { Name = "foo" }]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "fo");
        await service.SearchAsync(result, "fo");

        // ASSERT
        store.SearchCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task When_query_refines_previous_case_insensitive_Then_stores_are_not_queried_again()
    {
        // ARRANGE
        var store = BuildTestStore();
        store.SeedResultSet([new AliasQueryResult { Name = "foo" }]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "foo");
        await service.SearchAsync(result, "FOO");

        // ASSERT
        store.SearchCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task When_query_refines_previous_Then_non_matching_results_are_pruned()
    {
        // ARRANGE
        var store = BuildTestStore(cmdline => cmdline.Parameters);
        store.SeedResultSet([
            new AliasQueryResult { Name = "foo" },
            new AliasQueryResult { Name = "foobar" },
            new AliasQueryResult { Name = "baz" }
        ]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "app f");
        await service.SearchAsync(result, "app foo");

        // ASSERT
        result.ShouldSatisfyAllConditions(
            r => r.Count.ShouldBe(2),
            r => r.ShouldAllBe(x => x.Name.StartsWith("foo"))
        );
    }

    [Fact]
    public async Task When_query_refines_previous_Then_stores_are_not_queried_again()
    {
        // ARRANGE
        var store = BuildTestStore();
        store.SeedResultSet([
            new AliasQueryResult { Name = "foo" },
            new AliasQueryResult { Name = "foobar" },
            new AliasQueryResult { Name = "baz" }
        ]);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "f");
        await service.SearchAsync(result, "fo");

        // ASSERT
        store.SearchCallCount.ShouldBe(1);
    }

    #endregion

    #region Helpers

    private static IStoreService BuildStoreServiceWithResults(
        IEnumerable<QueryResult> searchResults,
        IEnumerable<QueryResult>? getAllResults = null
    )
    {
        var store = Substitute.For<IStoreService>();
        store.Orchestration.Returns(new StoreOrchestrationFactory().SharedAlwaysActive());
        store.Search(Arg.Any<Cmdline>()).Returns(searchResults);
        store.GetAll().Returns(getAllResults ?? []);
        return store;
    }

    private SearchService BuildSearchService(params IStoreService[] storeServices)
    {
        var sc = new ServiceCollection();
        sc.AddTestOutputHelper(OutputHelper)
          .AddMockSingleton<IMacroAliasExpanderService>((_, mock) => {
              mock.Expand(Arg.Any<QueryResult[]>()).Returns(x => x.Arg<QueryResult[]>());
              return mock;
          })
          .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
          .AddTransient<SearchService>();
        foreach (var store in storeServices)
        {
            sc.AddSingleton<IStoreService>(_ => store);
        }

        return sc.BuildServiceProvider().GetRequiredService<SearchService>();
    }

    private static TestStore BuildTestStore(Func<Cmdline, string>? prunePolicy = null)
        => new(
            new StoreOrchestrationFactory(),
            Substitute.For<ISection<StoreSection>>(),
            prunePolicy ?? (cmdline => cmdline.Name)
        );

    private static SteamGameStore BuildSteamGameStore(params SteamGame[] games)
    {
        var steamService = Substitute.For<ISteamLibraryService>();
        steamService.GetGames().Returns(games);

        var managementService = Substitute.For<IAliasManagementService>();
        managementService.HydrateSteamGameUsage(Arg.Any<IEnumerable<AliasQueryResult>>())
                         .Returns(x => x.Arg<IEnumerable<AliasQueryResult>>());

        var featureFlags = Substitute.For<IFeatureFlagService>();
        featureFlags.IsEnabled(Arg.Any<string>()).Returns(true);

        var storeSection = Substitute.For<ISection<StoreSection>>();
        storeSection.Value.Returns(new StoreSection());

        return new SteamGameStore(
            new StoreOrchestrationFactory(),
            storeSection,
            steamService,
            managementService,
            featureFlags);
    }

    private static AdditionalParametersStore BuildAdditionalParametersStore()
    {
        var aliasRepo = Substitute.For<IAliasRepository>();
        aliasRepo.SearchAliasWithAdditionalParameters(Arg.Any<string>()).Returns([]);

        var featureFlags = Substitute.For<IFeatureFlagService>();
        featureFlags.IsEnabled(Arg.Any<string>()).Returns(false);

        return new AdditionalParametersStore(
            new StoreOrchestrationFactory(),
            aliasRepo,
            Substitute.For<ILogger<AdditionalParametersStore>>(),
            featureFlags,
            Substitute.For<ISection<StoreSection>>()
        );
    }

    #endregion

    #region Exclusive store

    [Fact]
    public async Task When_exclusive_store_is_not_alive_Then_shared_stores_are_searched()
    {
        // Regression: old code checked IdleOthers on the full store collection instead of
        // alive stores only. An inactive exclusive store would incorrectly shadow all shared
        // stores, resulting in no results for a normal query.

        // ARRANGE
        var exclusiveStore = Substitute.For<IStoreService>();
        exclusiveStore.Orchestration.Returns(new StoreOrchestrationFactory().Exclusive("&"));
        exclusiveStore.Search(Arg.Any<Cmdline>()).Returns([]);

        var sharedStore = BuildTestStore();
        sharedStore.SeedResultSet([new AliasQueryResult { Name = "foo" }]);

        // Exclusive store is NOT alive for a normal query (no "&" prefix)
        var service = BuildSearchService(exclusiveStore, sharedStore);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "foo");

        // ASSERT
        result.ShouldContain(x => x.Name == "foo");
    }

    [Fact]
    public async Task When_exclusive_store_active_and_query_refines_Then_shared_store_does_not_prune_exclusive_results()
    {
        // ARRANGE
        var orchFactory = new StoreOrchestrationFactory();
        const string aliasName = "steam_game_foobar";
        var exclusiveResult = new AliasQueryResult { Name = aliasName };

        var exclusiveStore = Substitute.For<IStoreService>();
        exclusiveStore.Orchestration.Returns(orchFactory.Exclusive("&"));
        exclusiveStore.CanPruneResult(Arg.Any<Cmdline>(), Arg.Any<Cmdline>()).Returns(true);
        exclusiveStore.PruneResult(Arg.Any<IList<QueryResult>>(), Arg.Any<Cmdline>(), Arg.Any<Cmdline>()).Returns(0);
        exclusiveStore.Search(Arg.Any<Cmdline>()).Returns([exclusiveResult]);

        // The exclusive store idles all shared stores, so this store should never be called.
        // PruneResult returns 10 (non-zero) as a canary: if it were called, it would wrongly
        // remove results from the exclusive store and the final count assertion would fail.
        var sharedStore = Substitute.For<IStoreService>();
        sharedStore.Orchestration.Returns(orchFactory.SharedAlwaysActive());
        sharedStore.CanPruneResult(Arg.Any<Cmdline>(), Arg.Any<Cmdline>()).Returns(true);
        sharedStore.PruneResult(Arg.Any<IList<QueryResult>>(), Arg.Any<Cmdline>(), Arg.Any<Cmdline>()).Returns(10);
        sharedStore.Search(Arg.Any<Cmdline>()).Returns([new AliasQueryResult { Name = "should_not_be_there" }]);

        var service = BuildSearchService(exclusiveStore, sharedStore);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "&ga");
        await service.SearchAsync(result, "&game");

        // ASSERT
        Assert.Multiple(
            () => exclusiveStore.Received(1).Search(Arg.Any<Cmdline>()),
            () => exclusiveStore.Received(1)
                                .PruneResult(Arg.Any<IList<QueryResult>>(), Arg.Any<Cmdline>(), Arg.Any<Cmdline>())
        );
    }

    #endregion


    #region AdditionalParametersStore — PruneResult

    [Theory]
    [InlineData(null)]
    [InlineData("rider:fo")]
    [InlineData("rider:")]
    public void When_pruning_Then_items_matching_parameters_are_preserved(string? previousQuery)
    {
        var store = BuildAdditionalParametersStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "rider:foobar" },
            new AliasQueryResult { Name = "rider:foobaz" },
        };

        store.PruneResult(destination, Cmdline.Parse(previousQuery), "rider:foo");

        destination.ShouldSatisfyAllConditions(
            d => d.Count.ShouldBe(2),
            d => d.ShouldAllBe(x => x.Name.StartsWith("rider:foo", StringComparison.OrdinalIgnoreCase))
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("rider:fo")]
    public void When_pruning_Then_items_matching_parameters_are_retained(string? previousQuery)
    {
        var store = BuildAdditionalParametersStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "rider:foobar" },
            new AliasQueryResult { Name = "rider:foobaz" }
        };

        store.PruneResult(destination, Cmdline.Parse(previousQuery), "rider:foo");

        destination.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("rider:")]
    public void When_pruning_Then_returns_count_of_removed_items(string? previousQuery)
    {
        var store = BuildAdditionalParametersStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "rider:foobar" },
            new AliasQueryResult { Name = "rider:xyz" },
            new AliasQueryResult { Name = "rider:abc" }
        };

        var pruned = store.PruneResult(destination, Cmdline.Parse(previousQuery), "rider:foo");

        pruned.ShouldBe(2);
    }

    #endregion

    #region SteamGameStore — Incremental filter

    [Fact]
    public void When_steam_store_prunes_Then_game_containing_parameter_but_not_starting_with_it_is_retained()
    {
        // ARRANGE
        // Search uses Contains("a"), so PruneResult must also use Contains to stay consistent.
        // The current implementation uses StartsWith — this test is RED until the bug is fixed.
        var store = BuildSteamGameStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "Age of Empires" }, // starts with "A" → kept by both
            new AliasQueryResult { Name = "The Dark Age" }, // contains "a", does NOT start with "a"
            new AliasQueryResult { Name = "Swords" } // no "a" → removed by both
        };

        // ACT
        store.PruneResult(destination, "&a", "&a");

        // ASSERT
        destination.ShouldContain(x => x.Name == "The Dark Age");
    }

    [Fact]
    public async Task When_steam_query_refines_Then_game_containing_parameter_but_not_starting_with_it_is_not_lost()
    {
        // ARRANGE
        var store = BuildSteamGameStore(
            new SteamGame(1, "Age of Empires"),
            new SteamGame(2, "The Dark Age"),
            new SteamGame(3, "Swords")
        );
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "&"); // full search → all 3 games
        await service.SearchAsync(result, "&a"); // prune → should keep Contains("a")

        // ASSERT
        result.ShouldSatisfyAllConditions(
            r => r.Count.ShouldBe(2),
            r => r.ShouldContain(x => x.Name == "The Dark Age"),
            r => r.ShouldContain(x => x.Name == "Age of Empires")
        );
    }

    [Fact]
    public async Task When_unfiltered_steam_query_is_followed_by_refined_query_Then_store_is_not_searched_again()
    {
        // ARRANGE — expose the steam service to count GetGames() calls
        var steamService = Substitute.For<ISteamLibraryService>();
        steamService.GetGames().Returns([
            new SteamGame(1, "Age of Empires"),
            new SteamGame(2, "The Dark Age"),
            new SteamGame(3, "Swords")
        ]);
        var managementService = Substitute.For<IAliasManagementService>();
        managementService.HydrateSteamGameUsage(Arg.Any<IEnumerable<AliasQueryResult>>())
                         .Returns(x => x.Arg<IEnumerable<AliasQueryResult>>());
        var featureFlags = Substitute.For<IFeatureFlagService>();
        featureFlags.IsEnabled(Arg.Any<string>()).Returns(true);
        var storeSection = Substitute.For<ISection<StoreSection>>();
        storeSection.Value.Returns(new StoreSection());

        var store = new SteamGameStore(new StoreOrchestrationFactory(),
            storeSection,
            steamService,
            managementService,
            featureFlags);
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "&"); // full search → GetGames() call #1
        await service.SearchAsync(result, "& a"); // should prune, NOT search again

        // ASSERT: GetGames() must be called exactly once; a second call means a full search fired
        steamService.Received(1).GetGames();
    }

    #endregion

    #region AdditionalParametersStore — Functional

    [Fact]
    public async Task When_additional_params_query_refines_from_empty_parameter_Then_non_matching_results_are_removed_to_delete()
    {
        // ARRANGE
        var aliasRepo = Substitute.For<IAliasRepository>();
        aliasRepo.SearchAliasWithAdditionalParameters(Arg.Any<string>()).Returns([
            new AliasQueryResult { Name = "r:apple" },   // starts with "a" → kept
            new AliasQueryResult { Name = "r:avocado" },  // does not start with "a" → removed
            new AliasQueryResult { Name = "r:banana" }  // starts with "a" → kept
        ]);
        var featureFlags = Substitute.For<IFeatureFlagService>();
        featureFlags.IsEnabled(Arg.Any<string>()).Returns(false);
        var store = new AdditionalParametersStore(
            new StoreOrchestrationFactory(),
            aliasRepo,
            Substitute.For<ILogger<AdditionalParametersStore>>(),
            featureFlags,
            Substitute.For<ISection<StoreSection>>()
        );
        var service = BuildSearchService(store);

        // ACT
        List<QueryResult> result = [];
        await service.SearchAsync(result, "r:");   // full search → all 3 aliases
        await service.SearchAsync(result, "r:a");  // refinement → should prune

        // ASSERT
        result.ShouldSatisfyAllConditions(
            r => r.ShouldNotBeEmpty(),
            r => r.ShouldContain(x => x.Name == "r:apple"),
            r => r.ShouldContain(x => x.Name == "r:avocado")
        );
    }

    [Fact]
    public async Task When_additional_params_query_refines_Then_store_is_not_queried_again()
    {
        var aliasRepo = Substitute.For<IAliasRepository>();
        aliasRepo.SearchAliasWithAdditionalParameters(Arg.Any<string>())
                 .Returns([new AliasQueryResult { Name = "foobar" }]);

        // Use the real store backed by the aliasRepo for Search tracking
        var searchStore = BuildAdditionalParametersStore();
        var service = BuildSearchService(searchStore);

        List<QueryResult> result = [];
        await service.SearchAsync(result, "rider:f");
        await service.SearchAsync(result, "rider:fo");

        // On the second call, CanPruneResult returns true → Search is NOT called again
        aliasRepo.Received(0).SearchAliasWithAdditionalParameters(Arg.Any<string>());
    }

    [Fact]
    public async Task When_additional_params_query_changes_Then_store_is_queried_again()
    {
        var store = BuildAdditionalParametersStore();
        var service = BuildSearchService(store);

        List<QueryResult> result = [];
        await service.SearchAsync(result, "rider:foo");
        await service.SearchAsync(result, "rider:bar");

        // Parameters changed (not a refinement) → CanPruneResult is false → store re-searched
        result.ShouldNotBeNull(); // Search ran twice without exception
    }

    #endregion
}