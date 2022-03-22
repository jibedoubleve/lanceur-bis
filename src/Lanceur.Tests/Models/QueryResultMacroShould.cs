using FluentAssertions;
using Lanceur.Core;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models
{
    public class QueryResultMacroShould
    {
        #region Methods

        [Theory]
        [InlineData("calendar", false)]
        [InlineData("CaLeNdAr", false)]
        [InlineData("multi", true)]
        [InlineData("MuLtI", true)]
        public void BeAsExpected(string name, bool expected)
        {
            var item = new AliasQueryResult() { FileName = $"@{name}@" };

            item.Is(CompositeMacros.Multi).Should().Be(expected);
        }

        #endregion Methods
    }
}