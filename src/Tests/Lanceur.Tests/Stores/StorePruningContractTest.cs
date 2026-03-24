using System.Web.Bookmarks;
using Everything.Wrapper;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Stores;

public sealed class StorePruningContractTest
{
    #region Builders

    private static AliasStore BuildAliasStore()
        => new(
            new StoreOrchestrationFactory(),
            Substitute.For<IAliasRepository>(),
            Substitute.For<ILogger<AliasStore>>(),
            Substitute.For<ISection<StoreSection>>()
        );

    private static BookmarksStore BuildBookmarksStore()
        => new(
            new StoreOrchestrationFactory(),
            Substitute.For<IBookmarkRepositoryFactory>(),
            Substitute.For<ISection<StoreSection>>()
        );

    private static CalculatorStore BuildCalculatorStore()
        => new(
            new StoreOrchestrationFactory(),
            Substitute.For<ILogger<CalculatorStore>>(),
            Substitute.For<ICalculatorService>(),
            Substitute.For<ISection<StoreSection>>()
        );

    private static EverythingStore BuildEverythingStore()
        => new(
            new StoreOrchestrationFactory(),
            Substitute.For<ILogger<EverythingStore>>(),
            Substitute.For<IEverythingApi>(),
            Substitute.For<ISection<StoreSection>>()
        );

    #endregion

    #region AliasStore

    [Theory]
    [InlineData("fo", "foo", true)] // refinement (StartsWith) → can prune
    [InlineData("foo", "bar", false)] // not a refinement → cannot prune
    public void AliasStore_CanPruneResult(string previous, string current, bool expected)
        => BuildAliasStore().CanPruneResult(Cmdline.Parse(previous), current)
                            .ShouldBe(expected);

    [Fact]
    public void AliasStore_PruneResult_FiltersCorrectly()
    {
        var store = BuildAliasStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "foobar" }, // starts with "fo" AND "foo" → kept
            new AliasQueryResult { Name = "forest" }, // starts with "fo" but not "foo" → removed
            new AliasQueryResult { Name = "bar" } // doesn't start with "fo" → not "mine", kept
        };

        var removed = store.PruneResult(destination, "fo", "foo");

        removed.ShouldBe(1);
        destination.ShouldSatisfyAllConditions(
            d => d.ShouldContain(x => x.Name == "foobar"),
            d => d.ShouldNotContain(x => x.Name == "forest"),
            d => d.ShouldContain(x => x.Name == "bar")
        );
    }

    #endregion

    #region BookmarksStore

    [Theory]
    [InlineData("/ foo", "/ foobar", true)] // "foobar" contains "foo" → should be true,  currently false
    [InlineData("/ foo", "/ xyz", false)] // "xyz" doesn't contain "foo" → should be false, currently true
    [InlineData("/ oo", "/ goo", true)] // "goo" contains "oo" → can prune 
    [InlineData("/ ", "/ google", false)] // empty previous params → must trigger full search 
    public void BookmarksStore_CanPruneResult(string previous, string current, bool expected)
        => BuildBookmarksStore().CanPruneResult(Cmdline.Parse(previous), current)
                                .ShouldBe(expected);

    [Fact]
    public void BookmarksStore_PruneResult_FiltersCorrectly()
    {
        var store = BuildBookmarksStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "football scores" }, // contains "fo" AND "foo" → kept
            new AliasQueryResult { Name = "forest walk" }, // contains "fo" but not "foo" → removed
            new AliasQueryResult { Name = "xyz site" } // doesn't contain "fo" → not "mine", kept
        };

        var removed = store.PruneResult(destination, "/ fo", "/ foo");

        removed.ShouldBe(1);
        destination.ShouldSatisfyAllConditions(
            d => d.ShouldContain(x => x.Name == "football scores"),
            d => d.ShouldNotContain(x => x.Name == "forest walk"),
            d => d.ShouldContain(x => x.Name == "xyz site")
        );
    }

    [Fact]
    public void BookmarksStore_PruneResult_ShouldRemoveItemsThatMatchPreviousButNotCurrentFilter()
    {
        var store = BuildBookmarksStore();

        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "Google" }, // contains "oo" but NOT "ooh" → removed
            new AliasQueryResult { Name = "Doohan" }, // contains "ooh" → kept
            // "xyz" is owned by another store — this store ignores it and leaves it untouched.
            new AliasQueryResult { Name = "xyz" } // contains neither "oo" nor "ooh" → not owned by this store, kept
        };

        var removed = store.PruneResult(destination, "/ oo", "/ ooh");

        removed.ShouldBe(1);
        destination.ShouldSatisfyAllConditions(
            d => d.ShouldNotContain(x => x.Name == "Google"),
            d => d.ShouldContain(x => x.Name == "Doohan"),
            d => d.ShouldContain(x => x.Name == "xyz")
        );
    }

    #endregion

    #region CalculatorStore

    [Theory]
    [InlineData("1+", "1+2", false)] // CalculatorStore always returns false — each expression is unique
    [InlineData("1+2", "3*4", false)]
    public void CalculatorStore_CanPruneResult(string previous, string current, bool expected)
        => BuildCalculatorStore().CanPruneResult(Cmdline.Parse(previous), current)
                                 .ShouldBe(expected);

    [Theory]
    [InlineData("1+", "1+2")]
    [InlineData("", "1+2")]
    [InlineData(null, "1+2")]
    [InlineData("aaa", "1+2")]
    public void CalculatorStore_PruneResult_FiltersCorrectly(string? previous, string current)
    {
        // CalculatorStore uses StoreBase default: Name.StartsWith
        var store = BuildCalculatorStore();
        List<QueryResult> destination = [];

        var prev = previous is null ? null : Cmdline.Parse(previous);
        var removed = store.PruneResult(destination, prev, current);
        removed.ShouldBe(0);
    }

    #endregion

    #region EverythingStore

    [Theory]
    [InlineData(": fo", ": foo", false)] // EverythingStore always returns false — results are non-deterministic
    [InlineData(": foo", ": bar", false)]
    public void EverythingStore_CanPruneResult(string previous, string current, bool expected)
        => BuildEverythingStore().CanPruneResult(Cmdline.Parse(previous), current)
                                 .ShouldBe(expected);

    [Fact]
    public void EverythingStore_PruneResult_FiltersCorrectly()
    {
        // EverythingStore uses StoreBase default: Name.StartsWith.
        // Note: realistic Everything queries (": term") always have Name=":", so prevValue = currValue
        // and PruneResult never removes anything in practice. These tests use simple queries
        // to verify the StoreBase delegation is intact.
        var store = BuildEverythingStore();
        var destination = new List<QueryResult>
        {
            new AliasQueryResult { Name = "foobar" }, // starts with "fo" AND "foo" → kept
            new AliasQueryResult { Name = "forest" }, // starts with "fo" but not "foo" → removed
            new AliasQueryResult { Name = "bar" } // doesn't start with "fo" → not "mine", kept
        };

        var removed = store.PruneResult(destination, "fo", "foo");

        removed.ShouldBe(1);
        destination.ShouldSatisfyAllConditions(
            d => d.ShouldContain(x => x.Name == "foobar"),
            d => d.ShouldNotContain(x => x.Name == "forest"),
            d => d.ShouldContain(x => x.Name == "bar")
        );
    }

    #endregion
}