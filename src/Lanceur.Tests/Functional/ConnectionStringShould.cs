using FluentAssertions;
using Lanceur.Utils;
using System.Text.RegularExpressions;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class ConnectionStringShould
    {
        #region Methods

        [Fact(Skip = "Too complicated to fix and it'll be replaced by SpecFlow")]
        public void HaveExistingDebugDatabase()
        {
            var pattern = "Data Source=(.*);Version=3";
            var cs = new DebugConnectionString();
            var dbPAth = new Regex(pattern).Match(cs.ToString()).Groups[1].Value;

            File.Exists(dbPAth).Should().BeTrue();
        }

        [Fact]
        public void NotThrowErrorWhenFileExists()
        {
            //Arrange
            var file = Path.GetTempFileName();
            var cs = new ConnectionString(file);

            //Act
            var action = () => cs.ToString();

            // Assert
            action.Should().NotThrow<InvalidDataException>();

            //Tear down
            File.Delete(file);
        }

        [Fact]
        public void NotThrowErrorWhenFileDoesNotExist()
        {
            // Arrange
            var file = Path.GetTempFileName();
            File.Delete(file);

            var cs = new ConnectionString(file);

            //Act
            var action = () => cs.ToString();

            //Assert
            action.Should().NotThrow();
        }

        #endregion Methods
    }
}