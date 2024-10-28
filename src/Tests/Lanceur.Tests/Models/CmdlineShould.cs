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

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("un deux trois")]
    [InlineData("un")]
    public void BeEquals(string cmd)
    {
        var left = Cmdline.BuildFromText(cmd);
        var right = Cmdline.BuildFromText(cmd);
        
        (left == right).Should().BeTrue();
    } 
    
    [Theory]
    [InlineData("", "a")]
    [InlineData("un", "deux trois")]
    [InlineData("un", null)]
    [InlineData("un deux trois", "un deux")]
    [InlineData("deux trois", "deux quatre")]
    public void NotBeEquals(string cmd1, string cmd2)
    {
        var left = Cmdline.BuildFromText(cmd1);
        var right =Cmdline.BuildFromText(cmd2);
        
        (left != right).Should().BeTrue();
    }

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