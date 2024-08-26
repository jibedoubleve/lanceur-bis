using FluentAssertions;
using Lanceur.Core.Managers;
using Xunit;

namespace Lanceur.Tests.BusinessLogic;

public class CommandlineShould
{
    #region Methods

    [Theory, InlineData("arg1 arg2", "cls arg1 arg2"), InlineData("arg1 arg2", "excel arg1 arg2"), InlineData("", "excel"), InlineData("arg1 arg2", "% arg1 arg2"), InlineData("arg1 arg2", "$arg1 arg2"), InlineData("arg1 arg2", "? arg1 arg2"), InlineData("arg1 arg2", "?arg1 arg2"), InlineData("a?rg2", "arg1 a?rg2")]
    public void HaveArguments(string asExpected, string actual)
    {
        var line = CmdlineManager.BuildFromText(actual);

        line.Parameters.Should().Be(asExpected);
    }

    [Theory, InlineData("cls", "cls arg1 arg2"), InlineData("excel", "excel arg1 arg2"), InlineData("excel", "excel"), InlineData("%", "% arg1 arg2"), InlineData("$", "$arg1 arg2"), InlineData("a", "a")]
    public void HaveName(string asExpected, string actual)
    {
        var line = CmdlineManager.BuildFromText(actual);

        line.Name.Should().Be(asExpected);
    }

    [Theory, InlineData("? hello world", "?"), InlineData("??", "??"), InlineData("&& un deux trois", "&&"), InlineData("&", "&"), InlineData("", ""), InlineData("m&", "m&"), InlineData("m&&", "m&&"), InlineData("m& fff", "m&"), InlineData("m&& fff", "m&&")]
    public void RecogniseDoubleOrSingleSpecialChar(string cmdline, string expected)
    {

        CmdlineManager.BuildFromText(cmdline)
                      .Name
                      .Should()
                      .Be(expected);
    }

    [Fact]
    public void ReturnsEmptyOnEmptyCmdline()
    {
        var line = CmdlineManager.BuildFromText(string.Empty);

        line.Name.Should().BeEmpty();
        line.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void ReturnsEmptyOnNullCmdline()
    {
        var line = CmdlineManager.BuildFromText(null);

        line.Name.Should().BeEmpty();
        line.Parameters.Should().BeEmpty();
    }

    #endregion Methods
}