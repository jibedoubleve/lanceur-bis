using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public sealed class QueryResultTest
{
    #region Methods

    [Fact]
    public void When_created_Then_name_is_empty() => new TestQueryResult().Name.ShouldBeEmpty();

    [Fact]
    public void When_created_Then_originating_query_is_not_null()
    {
        var query = new TestQueryResult().OriginatingQuery;
        query.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null, "un", "un")]
    [InlineData("un", null, "un")]
    [InlineData(null, null, "")]
    public void When_description_is_missing_Then_file_name_is_displayed(string description, string fileName, string expected)
    {
        // ARRANGE
        var queryResult = new AliasQueryResult { Description = description, FileName = fileName };

        // ACT && ASSERT
        queryResult.DescriptionDisplay.ShouldBe(expected);
    }

    [Fact]
    public void When_created_Then_description_is_null() => new TestQueryResult().Description.ShouldBeNull();

    [Theory]
    [InlineData("un, deux,trois,quatre,cinq", 5)]
    [InlineData(" un, deux,trois,quatre,cinq ", 5)]
    [InlineData("un, deux,trois,quatre,cinq ", 5)]
    [InlineData(" un, deux,trois,quatre,cinq", 5)]
    public void When_splitting_synonyms_Then_names_are_trimmed(string synonyms, int count)
    {
        // ARRANGE
        var queryResult = new AliasQueryResult { Synonyms = synonyms };

        // ACT
        var names = queryResult.Synonyms.SplitCsv();

        // ASSERT
        names.ShouldSatisfyAllConditions(
            n => n.Length.ShouldBe(count),
            n => Assert.All(n, x => x.ShouldNotStartWith(" ")),
            n => Assert.All(n, x => x.ShouldNotEndWith(" "))
        );
    }

    [Fact]
    public void When_created_Then_count_is_zero() => new TestQueryResult().Count.ShouldBe(0);

    #endregion

    #region Classes

    private sealed class TestQueryResult : QueryResult { }

    #endregion Classes
}