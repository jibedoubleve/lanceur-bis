using FluentAssertions;
using Lanceur.Infra.SQLite;
using System.Data.SQLite;
using Xunit;

namespace Lanceur.Tests.SQLite
{
    public class ScopeConnectionShould
    {
        #region Methods

        [Fact]
        public void CrashWhenConnectionIsNull()
        {
            var newScope = () => new SQLiteConnectionScope((SQLiteConnection)null);

            newScope.Should().Throw<ArgumentNullException>();
        }
        [Fact]
        public void CrashWhenConnectionStringIsNull()
        {
            var newScope = () => new SQLiteConnectionScope((string)null);

            newScope.Should().Throw<ArgumentNullException>();
        }
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void CrashWhenConnectionStringIs(string value)
        {
            var newScope = () => new SQLiteConnectionScope(value);

            newScope.Should().Throw<ArgumentNullException>();
        }

        #endregion Methods
    }
}