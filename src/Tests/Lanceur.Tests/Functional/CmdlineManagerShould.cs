using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Tests.Tooling.Macros;
using Xunit;

namespace Lanceur.Tests.Functional;

public partial class CmdlineManagerShould
{
    #region Methods

    [Theory, InlineData("ini", ""), InlineData("ini un deux trois", "un deux trois"), InlineData("ini", null)]
    public void BuildCommand(string cmdline, string parameters)
    {
        parameters ??= string.Empty;
        var macro = new MultiMacroTest(parameters);

        var cmd = CmdlineManager.Build(cmdline, macro);

        cmd.Parameters.Should().Be(parameters);
    }

    [Theory, InlineData("ini", "un deux trois", "un deux trois"), InlineData("ini sept huit neuf", "dix onze", "sept huit neuf")]
    public void BuildCommandWithParameters(string cmdline, string macroParams, string expected)
    {
        expected ??= string.Empty;
        var macro = new MultiMacroTest(macroParams);

        var cmd = CmdlineManager.Build(cmdline, macro);

        cmd.Parameters.Should().Be(expected);
    }

    [Theory, InlineData("init", "un deux trois", "quatre cinq six"), InlineData("move", "un", "quatre cinq six"), InlineData("move", "", "quatre cinq six"), InlineData("", "", "quatre cinq six"), InlineData("?", "", "quatre cinq six"), InlineData("", "un deux trois", "quatre cinq six"), InlineData("?", "un deux trois", "quatre cinq six")]
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

    [Theory, InlineData("un deux trois"), InlineData("quatre cinq six")]
    public void CloneCmdlineWithEmptyParameters(string newParameters)
    {
        // Arrange
        var name = "init";
        var parameters = "un deux trois";
        var cmd = CmdlineManager.BuildFromText($"{name} {parameters}");

        // Act
        var newCmd = CmdlineManager.CloneWithNewParameters(newParameters, cmd);

        // Assert
        newCmd.Name.Should().Be(name);
        newCmd.Parameters.Should().Be(newParameters);
    }

    #endregion Methods
}