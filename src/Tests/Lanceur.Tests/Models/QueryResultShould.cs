using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;
using Xunit;

namespace Lanceur.Tests.Models;

public class QueryResultShould
{
    #region Methods

    [Fact] public void HaveEmptyNameByDefault() => new TestQueryResult().Name.Should().BeEmpty();

    [Fact]
    public void HaveEmptyQueryByDefault()
    {
        var query = new TestQueryResult().Query;
        query.Should().NotBeNull();
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
        queryResult.DescriptionDisplay.Should().Be(expected);
    }

    [Fact] public void HaveNullDescriptionByDefault() { new TestQueryResult().Description.Should().BeNull(); }

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
        using (new AssertionScope())
        {
            names.Should().HaveCount(count);
            _ = names.Select(x => x.Should().NotStartWith(" "));
            _ = names.Select(x => x.Should().NotEndWith(" "));
        }
    }

    [Fact] public void HaveZeroCountByDefault() { new TestQueryResult().Count.Should().Be(0); }

    #endregion

    #region Classes

    private class TestQueryResult : QueryResult { }

    #endregion Classes
}