﻿using FluentAssertions;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Managers;
using Lanceur.Tests.Tooling.Macros;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public partial class CmdlineManagerShould
{
    #region Methods

    [Theory]
    [InlineData("init", "un deux trois", "quatre cinq six")]
    [InlineData("move", "un", "quatre cinq six")]
    [InlineData("move", "", "quatre cinq six")]
    [InlineData("", "", "quatre cinq six")]
    [InlineData("?", "", "quatre cinq six")]
    [InlineData("", "un deux trois", "quatre cinq six")]
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