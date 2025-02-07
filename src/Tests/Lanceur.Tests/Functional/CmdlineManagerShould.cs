using FluentAssertions;
using Lanceur.Core.Managers;
using Xunit;

namespace Lanceur.Tests.Functional;

public class CmdlineManagerShould
{
    #region Methods

    [Theory]
    [InlineData("init", "un deux trois", "quatre cinq six")]
    [InlineData("move", "un", "quatre cinq six")]
    [InlineData("move", "", "quatre cinq six")]
    [InlineData("?", "", "quatre cinq six")]
    [InlineData("?", "un deux trois", "quatre cinq six")]
    public void CloneCmdline(string name, string parameters, string newParameters)
    {
        // Arrange
        var cmdline = $"{name} {parameters}";
        var cmd = CmdlineManager.BuildFromText(cmdline);

        // Act
        var newCmd = CmdlineManager.CloneWithNewParameters(newParameters, cmd);

        // Assert
        newCmd.Name.Should().Be(name);
        newCmd.Parameters.Should().Be(newParameters);
    }

    [Theory]
    [InlineData("un deux trois")]
    [InlineData("quatre cinq six")]
    public void CloneCmdlineWithEmptyParameters(string newParameters)
    {
        // Arrange
        const string name = "init";
        const string parameters = "un deux trois";
        var cmd = CmdlineManager.BuildFromText($"{name} {parameters}");

        // Act
        var newCmd = CmdlineManager.CloneWithNewParameters(newParameters, cmd);

        // Assert
        newCmd.Name.Should().Be(name);
        newCmd.Parameters.Should().Be(newParameters);
    }

    #endregion Methods
}