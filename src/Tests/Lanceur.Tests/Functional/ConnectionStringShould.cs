using FluentAssertions;
using Lanceur.Utils.ConnectionStrings;
using System.Text.RegularExpressions;
using Xunit;

namespace Lanceur.Tests.Functional
{
    public class ConnectionStringShould
    {
        #region Methods

        [Fact]
        public void HaveExistingDebugDatabase()
        {
            // ARRANGE
            var file = Path.GetTempFileName();
            var pattern = "Data Source=(.*);Version=3";
            var cs = DebugConnectionString.FromFile(file);

            // ACT
            var dbPAth = new Regex(pattern).Match(cs.ToString()).Groups[1].Value;

            // ASSERT
            dbPAth.Should().Be(file);
        }

        [Fact]
        public void NotThrowErrorWhenFileDoesNotExist()
        {
            // Arrange
            var file = Path.GetTempFileName();
            File.Delete(file);

            var cs = new ConnectionString(file);

            // Act
            var action = () => cs.ToString();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void NotThrowErrorWhenFileExists()
        {
            // Arrange
            var file = Path.GetTempFileName();
            var cs = new ConnectionString(file);

            // Act
            var action = () => cs.ToString();

            // Assert
            action.Should().NotThrow<InvalidDataException>();

            // Tear down
            File.Delete(file);
        }

        #endregion Methods
    }
}