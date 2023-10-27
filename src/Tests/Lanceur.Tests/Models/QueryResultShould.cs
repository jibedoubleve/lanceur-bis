using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;
using Xunit;

namespace Lanceur.Tests.Models
{
    public class QueryResultShould
    {
        #region Methods

        [Fact]
        public void HaveEmptyNameByDefault() => new TestQueryResult().Name.Should().BeEmpty();

        [Fact]
        public void HaveEmptyQueryByDefault()
        {
            var query = new TestQueryResult().Query;
            query.Should().NotBeNull();
        }

        [Fact]
        public void HaveNullDescriptionByDefault()
        {
            new TestQueryResult().Description.Should().BeNull();
        }

        [Fact]
        public void HaveTrimmedResultInSynonyms()
        {
            // arrange
            var synonyms = "un, deux,trois,quatre,cinq";
            var queryResult = new AliasQueryResult() { Synonyms = synonyms };

            // act
            var names = queryResult.Synonyms.SplitCsv();

            // assert
            using (new AssertionScope())
            {
                names.Should().HaveCount(5);
                names.Select(x => x.Should().NotStartWith(" "));
                names.Select(x => x.Should().NotEndWith(" "));
            }
        }

        [Fact]
        public void HaveZeroCountByDefault()
        {
            new TestQueryResult().Count.Should().Be(0);
        }

        #endregion Methods

        #region Classes

        public class TestQueryResult : QueryResult
        { }

        #endregion Classes
    }
}