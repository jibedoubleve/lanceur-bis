using Shouldly;
using System.Text.RegularExpressions;
using Lanceur.Core.Configuration;
using Lanceur.Core.Repositories.Config;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
        dbPAth.ShouldBe(file);
    }

    [Fact]
    public void NotThrowErrorWhenFileDoesNotExist()
    {
        // Arrange
        var config = Substitute.For<IApplicationConfigurationService>();
        config.Current.Returns(new ApplicationSettings { DbPath = "lkj" }); 
        var logger = Substitute.For<ILogger<ConnectionString>>();
        var cs = new ConnectionString(config, logger);

        // Act
        var action = () => cs.ToString();

        // Assert
        action.ShouldNotThrow();
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
        config.Current.Returns(new ApplicationSettings { DbPath = file });
        var logger = Substitute.For<ILogger<ConnectionString>>();
        var cs = new ConnectionString(config, logger);

        // Act
        var action = () => cs.ToString();

        // Assert
        action.ShouldNotThrow();

        //Teardown
        File.Delete(file);
    }

    #endregion Methods
}