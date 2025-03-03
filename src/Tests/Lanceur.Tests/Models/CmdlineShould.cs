using FluentAssertions;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models;

public class CmdlineShould
{
    #region Methods

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("un deux trois")]
    [InlineData("un")]
    public void BeEquals(string cmd)
    {
        var left = Cmdline.Parse(cmd);
        var right = Cmdline.Parse(cmd);

        (left == right).Should().BeTrue();
    }

    [Theory]
    [InlineData("cmd", "cmd", "")]
    [InlineData("cmd", " cmd ", "")]
    [InlineData("cmd", "  cmd  ", "")]
    [InlineData("cmd", "cmd", " arg1 arg2")]
    [InlineData("cmd", "cmd", "     arg1 arg2")]
    [InlineData("%", "%", " arg1 arg2")]
    [InlineData("$", "$", "arg1 arg2")]
    public void Build(string asExpected, string cmd, string args)
    {
        var line = new Cmdline(cmd, args);

        line.Name.Should().Be(asExpected);
    }

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
        var cmd = Cmdline.Parse(cmdline);

        // Act
        var newCmd = Cmdline.CloneWithNewParameters(newParameters, cmd);

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
        var cmd = Cmdline.Parse($"{name} {parameters}");

        // Act
        var newCmd = Cmdline.CloneWithNewParameters(newParameters, cmd);

        // Assert
        newCmd.Name.Should().Be(name);
        newCmd.Parameters.Should().Be(newParameters);
    }

    [Theory]
    [InlineData("$AAAA aa")]
    [InlineData("&AAAA aa")]
    [InlineData("|AAAA aa")]
    [InlineData("@AAAA aa")]
    [InlineData("#AAAA aa")]
    [InlineData("(AAAA aa")]
    [InlineData(")AAAA aa")]
    [InlineData("§AAAA aa")]
    [InlineData("!AAAA aa")]
    [InlineData("{AAAA aa")]
    [InlineData("}AAAA aa")]
    [InlineData("-AAAA aa")]
    [InlineData("_AAAA aa")]
    [InlineData("\\AAAA aa")]
    [InlineData("+AAAA aa")]
    [InlineData("*AAAA aa")]
    [InlineData("/AAAA aa")]
    [InlineData("=AAAA aa")]
    [InlineData("<AAAA aa")]
    [InlineData(">AAAA aa")]
    [InlineData(",AAAA aa")]
    [InlineData(";AAAA aa")]
    [InlineData(":AAAA aa")]
    [InlineData("%AAAA aa")]
    [InlineData("?AAAA aa")]
    [InlineData(".AAAA aa")]
    public void HandleSpecialCmdCharacter(string cmdline)
    {
        Cmdline.Parse(cmdline)
               .Name.Should()
               .Be(cmdline[0].ToString());
    }

    [Theory]
    [InlineData("arg1 arg2", "cls arg1 arg2")]
    [InlineData("arg1 arg2", "excel arg1 arg2")]
    [InlineData("", "excel")]
    [InlineData("arg1 arg2", "% arg1 arg2")]
    [InlineData("arg1 arg2", "$arg1 arg2")]
    [InlineData("arg1 arg2", "? arg1 arg2")]
    [InlineData("arg1 arg2", "?arg1 arg2")]
    [InlineData("a?rg2", "arg1 a?rg2")]
    public void HaveArguments(string asExpected, string actual)
    {
        var line = Cmdline.Parse(actual);

        line.Parameters.Should().Be(asExpected);
    }

    [Fact] public void HaveEmptyNameByDefault() { Cmdline.Empty.Name.Should().BeEmpty(); }

    [Fact] public void HaveEmptyNameWhenCtorNull() { new Cmdline(null, null).Name.Should().BeEmpty(); }

    [Fact] public void HaveEmptyParametersByDefault() { Cmdline.Empty.Parameters.Should().BeEmpty(); }

    [Fact] public void HaveEmptyParametersWhenCtorNull() { new Cmdline(null, null).Parameters.Should().BeEmpty(); }

    [Theory]
    [InlineData("cmd", "cmd")]
    [InlineData("cmd", " cmd ")]
    [InlineData("cmd", "  cmd  ")]
    [InlineData("cmd", "cmd arg1 arg2")]
    [InlineData("cmd", "cmd     arg1 arg2")]
    [InlineData("%", "% arg1 arg2")]
    [InlineData("$", "$arg1 arg2")]
    public void HaveName(string asExpected, string actual)
    {
        var line = Cmdline.Parse(actual);

        line.Name.Should().Be(asExpected);
    }

    [Theory]
    [InlineData("", "a")]
    [InlineData("un", "deux trois")]
    [InlineData("un", null)]
    [InlineData("un deux trois", "un deux")]
    [InlineData("deux trois", "deux quatre")]
    public void NotBeEquals(string cmd1, string cmd2)
    {
        var left = Cmdline.Parse(cmd1?.Trim());
        var right = Cmdline.Parse(cmd2?.Trim());

        (left != right).Should().BeTrue();
    }

    [Theory]
    [InlineData("? hello world", "?")]
    [InlineData("??", "??")]
    [InlineData("&& un deux trois", "&&")]
    [InlineData("&", "&")]
    [InlineData("", "")]
    [InlineData("m&", "m&")]
    [InlineData("m&&", "m&&")]
    [InlineData("m& fff", "m&")]
    [InlineData("m&& fff", "m&&")]
    public void RecogniseDoubleOrSingleSpecialChar(string cmdline, string expected)
    {
        Cmdline.Parse(cmdline)
               .Name
               .Should()
               .Be(expected);
    }

    [Fact]
    public void ReturnsEmptyOnEmptyCmdline()
    {
        var line = Cmdline.Parse(string.Empty);

        line.Name.Should().BeEmpty();
        line.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void ReturnsEmptyOnNullCmdline()
    {
        var line = Cmdline.Parse(null);

        line.Name.Should().BeEmpty();
        line.Parameters.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  undeux  ")]
    [InlineData("  undeux")]
    [InlineData("undeux  ")]
    public void TrimLeadingAndTrailingWhitespace(string cmd)
    {
        Cmdline.Parse(cmd)
               .Name.Should()
               .Be(cmd.Trim());
    }

    #endregion
}