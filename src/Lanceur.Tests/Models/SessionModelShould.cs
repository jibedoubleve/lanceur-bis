using FluentAssertions;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models
{
    public class SessionModelShould
    {
        #region Methods

        [Theory]
        [InlineData("Un", "note 1")]
        [InlineData("Deux", "note 2")]
        [InlineData("Trois", "note 3")]
        [InlineData("Quatre", "note 4")]
        public void IndicateNoteBetweenParenthisesWhenNotes(string name, string notes)
        {
            var session = new Session { Name = name, Notes = notes };

            session.FullName.Should().Be($"{name} ({notes})");
        }

        [Theory]
        [InlineData("Un")]
        [InlineData("Deux")]
        [InlineData("Trois")]
        [InlineData("Quatre")]
        public void NotIndicateParenthesisWhenNoNote(string name)
        {
            var session = new Session { Name = name, Notes = string.Empty };

            session.FullName.Should().Be(name);
        }

        #endregion Methods
    }
}