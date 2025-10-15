using Shouldly;
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

        (left == right).ShouldBeTrue();
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

        line.Name.ShouldBe(asExpected);
    }

    [Theory]
    [InlineData("$AAAA aa")]
    [InlineData("&AAAA aa")]
    [InlineData("|AAAA aa")]
    [InlineData("@AAAA aa")]
    [InlineData("#AAAA aa")]
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
               .Name.ShouldBe(cmdline[0].ToString());
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

        line.Parameters.ShouldBe(asExpected);
    }

    [Fact] public void HaveEmptyNameByDefault() { Cmdline.Empty.Name.ShouldBeEmpty(); }

    [Fact] public void HaveEmptyNameWhenCtorNull() { new Cmdline(null, null).Name.ShouldBeEmpty(); }

    [Fact] public void HaveEmptyParametersByDefault() { Cmdline.Empty.Parameters.ShouldBeEmpty(); }

    [Fact] public void HaveEmptyParametersWhenCtorNull() { new Cmdline(null, null).Parameters.ShouldBeEmpty(); }

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

        line.Name.ShouldBe(asExpected);
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

        (left != right).ShouldBeTrue();
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
               .ShouldBe(expected);
    }

    [Fact]
    public void ReturnsEmptyOnEmptyCmdline()
    {
        var line = Cmdline.Parse(string.Empty);

        line.Name.ShouldBeEmpty();
        line.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ReturnsEmptyOnNullCmdline()
    {
        var line = Cmdline.Parse(null);

        line.Name.ShouldBeEmpty();
        line.Parameters.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  undeux  ")]
    [InlineData("  undeux")]
    [InlineData("undeux  ")]
    public void TrimLeadingAndTrailingWhitespace(string cmd)
    {
        Cmdline.Parse(cmd)
               .Name.ShouldBe(cmd.Trim());
    }

    #endregion
}