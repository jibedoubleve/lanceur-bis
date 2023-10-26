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
            var newScope = () => new SQLiteDbConnectionManager((SQLiteConnection)null);

            newScope.Should().Throw<ArgumentNullException>();
        }

        #endregion Methods
    }
}