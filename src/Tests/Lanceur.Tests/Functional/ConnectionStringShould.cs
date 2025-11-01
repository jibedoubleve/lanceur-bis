using System.Text.RegularExpressions;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Repositories.Config;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Ui.Core.Utils.ConnectionStrings;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public class ConnectionStringShould
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public ConnectionStringShould(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    private ILogger<ConnectionString> CreateLogger()
        => new TestOutputHelperDecoratorForMicrosoftLogging<ConnectionString>(_output);

    private string CreateTemporaryFile()
    {
        var tempFilePath = Path.GetTempFileName();
        File.WriteAllText(tempFilePath, "");
        return tempFilePath;
    }

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
        config.Current.Returns(new ApplicationConfiguration { DbPath = "lkj" });
        var cs = new ConnectionString(config, CreateLogger());

        // Act
        var action = () => cs.ToString();

        // Assert
        action.ShouldNotThrow();
    }

    [Fact]
    public void NotThrowErrorWhenFileExists()
    {
        // Arrange
        var file = CreateTemporaryFile();
        var config = Substitute.For<IApplicationConfigurationService>();
        config.Current.Returns(new ApplicationConfiguration { DbPath = file });
        var cs = new ConnectionString(config, CreateLogger());

        // Act
        var action = () => cs.ToString();

        // Assert
        action.ShouldNotThrow();

        //Teardown
        File.Delete(file);
    }

    #endregion
}