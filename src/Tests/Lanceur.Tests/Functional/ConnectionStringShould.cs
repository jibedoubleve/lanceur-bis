using FluentAssertions;
using System.Text.RegularExpressions;
using Lanceur.Core.Repositories.Config;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Routing.Handlers;
using Xunit;

namespace Lanceur.Tests.Functional;

public class ConnectionStringShould
{
    #region Methods

    [Fact]
    public void HaveExistingDebugDatabase()
    {
        // ARRANGE
        var file = Path.GetTempFileName();
        var pattern = "Data Source=(.*);Version=3";
        var cs = LightConnectionString.FromFile(file);

        // ACT
        var dbPAth = new Regex(pattern).Match(cs.ToString()).Groups[1].Value;

        // ASSERT
        dbPAth.Should().Be(file);
    }

    [Fact]
    public void NotThrowErrorWhenFileDoesNotExist()
    {
        // Arrange
        var config = Substitute.For<IApplicationConfigurationService>();
        config.Current.DbPath.Returns("lkj");
        var logger = Substitute.For<ILogger<ConnectionString>>();
        var cs = new ConnectionString(config, logger);

        // Act
        var action = () => cs.ToString();

        // Assert
        action.Should().NotThrow();
    }

    private string CreateTemporaryFile()
    {
        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, "");
        return tempFilePath;
    }

    [Fact]
    public void NotThrowErrorWhenFileExists()
    {
        // Arrange
        var file = CreateTemporaryFile();
        var config = Substitute.For<IApplicationConfigurationService>();
        config.Current.DbPath.Returns(file);
        var logger = Substitute.For<ILogger<ConnectionString>>();
        var cs = new ConnectionString(config, logger);

        // Act
        var action = () => cs.ToString();

        // Assert
        action.Should().NotThrow<InvalidDataException>();

        //Teardown
        File.Delete(file);
    }

    #endregion Methods
}