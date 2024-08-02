using FluentAssertions;
using Lanceur.Infra.Managers;
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
        var cmdlineManager = new CmdlineManager();
        var macro = new MultiMacroTest(parameters);

        var cmd = cmdlineManager.Build(cmdline, macro);

        cmd.Parameters.Should().Be(parameters);
    }

    [Theory, InlineData("ini", "un deux trois", "un deux trois"), InlineData("ini sept huit neuf", "dix onze", "sept huit neuf")]
    public void BuildCommandWithParameters(string cmdline, string macroParams, string expected)
    {
        expected ??= string.Empty;
        var mgr = new CmdlineManager();
        var macro = new MultiMacroTest(macroParams);

        var cmd = mgr.Build(cmdline, macro);

        cmd.Parameters.Should().Be(expected);
    }

    [Theory, InlineData("init", "un deux trois", "quatre cinq six"), InlineData("move", "un", "quatre cinq six"), InlineData("move", "", "quatre cinq six"), InlineData("", "", "quatre cinq six"), InlineData("?", "", "quatre cinq six"), InlineData("", "un deux trois", "quatre cinq six"), InlineData("?", "un deux trois", "quatre cinq six")]
    public void CloneCmdline(string name, string parameters, string newParameters)
    {
        // Arrange
        var mgr = new CmdlineManager();
        var cmdline = $"{name} {parameters}";
        var cmd = mgr.BuildFromText(cmdline);

        // Act
        var newCmd = mgr.CloneWithNewParameters(newParameters, cmd);

        // Assert
        newCmd.Name.Should().Be(name);
        newCmd.Parameters.Should().Be(newParameters);
    }

    [Theory, InlineData("un deux trois"), InlineData("quatre cinq six")]
    public void CloneCmdlineWithEmptyParameters(string newParameters)
    {
        // Arrange
        var mgr = new CmdlineManager();
        var name = "init";
        var parameters = "un deux trois";
        var cmd = mgr.BuildFromText($"{name} {parameters}");

        // Act
        var newCmd = mgr.CloneWithNewParameters(newParameters, cmd);

        // Assert
        newCmd.Name.Should().Be(name);
        newCmd.Parameters.Should().Be(newParameters);
    }

    #endregion Methods
}