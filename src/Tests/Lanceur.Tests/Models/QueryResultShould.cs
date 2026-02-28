using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public class QueryResultShould
{
    #region Methods

    [Fact] public void HaveEmptyNameByDefault() => new TestQueryResult().Name.ShouldBeEmpty();

    [Fact]
    public void HaveEmptyQueryByDefault()
    {
        var query = new TestQueryResult().OriginatingQuery;
        query.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null, "un", "un")]
    [InlineData("un", null, "un")]
    [InlineData(null, null, null)]
    public void HaveFileNameAsDescriptionWhenNoDescription(string description, string fileName, string expected)
    {
        // ARRANGE
        var queryResult = new AliasQueryResult { Description = description, FileName = fileName };

        // ACT && ASSERT
        queryResult.DescriptionDisplay.ShouldBe(expected);
    }

    [Fact] public void HaveNullDescriptionByDefault() { new TestQueryResult().Description.ShouldBeNull(); }

    [Theory]
    [InlineData("un, deux,trois,quatre,cinq", 5)]
    [InlineData(" un, deux,trois,quatre,cinq ", 5)]
    [InlineData("un, deux,trois,quatre,cinq ", 5)]
    [InlineData(" un, deux,trois,quatre,cinq", 5)]
    public void HaveTrimmedResultInSynonyms(string synonyms, int count)
    {
        // ARRANGE
        var queryResult = new AliasQueryResult { Synonyms = synonyms };

        // ACT
        var names = queryResult.Synonyms.SplitCsv();

        // ASSERT
        names.ShouldSatisfyAllConditions(
            n =>  n.Length.ShouldBe(count),
            n =>  Assert.All(n, x => x.ShouldNotStartWith(" ")),
            n =>  Assert.All(n, x => x.ShouldNotEndWith(" "))
        );
    }

    [Fact] public void HaveZeroCountByDefault() { new TestQueryResult().Count.ShouldBe(0); }

    #endregion

    #region Classes

    private class TestQueryResult : QueryResult { }

    #endregion Classes
}