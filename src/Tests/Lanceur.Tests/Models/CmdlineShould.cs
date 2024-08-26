using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models;

public class CmdlineShould
{
    #region Methods

    [Fact]
    public void HaveEmptyNameByDefault() { Cmdline.Empty.Name.Should().BeEmpty(); }

    [Fact]
    public void HaveEmptyNameWhenCtorNull() { new Cmdline(null, null).Name.Should().BeEmpty(); }

    [Fact]
    public void HaveEmptyParametersByDefault() { Cmdline.Empty.Parameters.Should().BeEmpty(); }

    [Fact]
    public void HaveEmptyParametersWhenCtorNull() { new Cmdline(null, null).Parameters.Should().BeEmpty(); }

    [Theory,
     InlineData("$AAAA aa"),
     InlineData("&AAAA aa"),
     InlineData("|AAAA aa"),
     InlineData("@AAAA aa"),
     InlineData("#AAAA aa"),
     InlineData("(AAAA aa"),
     InlineData(")AAAA aa"),
     InlineData("§AAAA aa"),
     InlineData("!AAAA aa"),
     InlineData("{AAAA aa"),
     InlineData("}AAAA aa"),
     InlineData("-AAAA aa"),
     InlineData("_AAAA aa"),
     InlineData("\\AAAA aa"),
     InlineData("+AAAA aa"),
     InlineData("*AAAA aa"),
     InlineData("/AAAA aa"),
     InlineData("=AAAA aa"),
     InlineData("<AAAA aa"),
     InlineData(">AAAA aa"),
     InlineData(",AAAA aa"),
     InlineData(";AAAA aa"),
     InlineData(":AAAA aa"),
     InlineData("%AAAA aa"),
     InlineData("?AAAA aa"),
     InlineData(".AAAA aa")]
    public void HandleSpecialCmdCharacter(string cmdline)
    {
        CmdlineManager.BuildFromText(cmdline)
                      .Name.Should()
                      .Be(cmdline[0].ToString());
    }

    #endregion Methods
}