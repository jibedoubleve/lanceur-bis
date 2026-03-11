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

public class SteamGameStoreTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public SteamGameStoreTest(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    private SteamGameStore GetStore(ISteamLibraryService steamService, IAliasManagementService managementService)
    {
        var sp = new ServiceCollection()
                 .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                 .AddSingleton(steamService)
                 .AddSingleton(managementService)
                 .AddSingleton<SteamGameStore>()
                 .AddMockSingleton<IFeatureFlagService>((_, i)=> {
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
    [InlineData("&half", 1)] // Shoulf return "Half-Life 2"
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