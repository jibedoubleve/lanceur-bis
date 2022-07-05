using FluentAssertions;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models
{
    public class QueryResultShould
    {
        #region Methods

        [Fact]
        public void HaveNullDescriptionByDefault()
        {
            new TestQueryResult().Description.Should().BeNull();
        }

        [Fact]
        public void HaveEmptyNameByDefault()
        {
            new TestQueryResult().Name.Should().BeEmpty();
        }

        [Fact]
        public void HaveEmptyQueryByDefault()
        {
            var query = new TestQueryResult().Query;
            query.Should().NotBeNull();
        }

        [Fact]
        public void HaveZeroCountByDefault()
        {
            new TestQueryResult().Count.Should().Be(0);
        }

        [Theory]
        [InlineData("", "bonjour tout le monde")]
        [InlineData("hello world", "bonjour tout le monde")]
        public void HaveOldNameUnchangedWhenUpdateName(string expected, string actual)
        {
            var query = new TestQueryResult()
            {
                Name = expected
            };
            query.Name = actual;
            query.OldName.Should().Be(expected);
        }

        [Fact]
        public void HaveOldNameChangedWhenNullWasProvidedTheFirstTime()
        {
            var name = "bonjour tout le monde";
            var query = new TestQueryResult() { Name = null };
            query.Name = name;

            query.OldName.Should().Be(name);


        }

        #endregion Methods

        #region Classes

        public class TestQueryResult : QueryResult
        { }

        #endregion Classes
    }
}