using Lanceur.Core.Models;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public sealed class CmdlineTest
{
    #region Methods

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("un deux trois")]
    [InlineData("un")]
    public void When_parsing_same_text_twice_Then_cmdlines_are_equal(string cmd)
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
    public void When_creating_with_constructor_Then_name_is_trimmed(string asExpected, string cmd, string args)
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
    public void When_text_starts_with_special_char_Then_name_is_that_char(string cmdline) =>
        Cmdline.Parse(cmdline)
               .Name.ShouldBe(cmdline[0].ToString());

    [Theory]
    [InlineData("arg1 arg2", "cls arg1 arg2")]
    [InlineData("arg1 arg2", "excel arg1 arg2")]
    [InlineData("", "excel")]
    [InlineData("arg1 arg2", "% arg1 arg2")]
    [InlineData("arg1 arg2", "$arg1 arg2")]
    [InlineData("arg1 arg2", "? arg1 arg2")]
    [InlineData("arg1 arg2", "?arg1 arg2")]
    [InlineData("a?rg2", "arg1 a?rg2")]
    public void When_parsing_Then_parameters_are_extracted(string asExpected, string actual)
    {
        var line = Cmdline.Parse(actual);

        line.Parameters.ShouldBe(asExpected);
    }

    [Fact]
    public void When_using_Empty_Then_name_is_empty() => Cmdline.Empty.Name.ShouldBeEmpty();

    [Fact]
    public void When_using_Empty_Then_parameters_are_empty() => Cmdline.Empty.Parameters.ShouldBeEmpty();

    [Theory]
    [InlineData("cmd", "cmd")]
    [InlineData("cmd", " cmd ")]
    [InlineData("cmd", "  cmd  ")]
    [InlineData("cmd", "cmd arg1 arg2")]
    [InlineData("cmd", "cmd     arg1 arg2")]
    [InlineData("%", "% arg1 arg2")]
    [InlineData("$", "$arg1 arg2")]
    public void When_parsing_Then_name_is_extracted(string asExpected, string actual)
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
    public void When_parsing_different_text_Then_cmdlines_are_not_equal(string cmd1, string cmd2)
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
    public void When_parsing_single_or_double_special_char_Then_name_keeps_them(string cmdline, string expected) =>
        Cmdline.Parse(cmdline)
               .Name
               .ShouldBe(expected);

    [Fact]
    public void When_parsing_empty_text_Then_name_and_parameters_are_empty()
    {
        var line = Cmdline.Parse(string.Empty);

        line.Name.ShouldBeEmpty();
        line.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void When_parsing_null_Then_name_and_parameters_are_empty()
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
    public void When_text_has_surrounding_spaces_Then_name_is_trimmed(string cmd) =>
        Cmdline.Parse(cmd)
               .Name.ShouldBe(cmd.Trim());

    #endregion
}