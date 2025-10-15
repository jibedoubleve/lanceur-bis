using Shouldly;
using Lanceur.Core.Models;
using Xunit;

namespace Lanceur.Tests.Models;

public class QueryResultMacroShould
{
    #region Methods

    [Theory]
    [InlineData("calendar", false)]
    [InlineData("CaLeNdAr", false)]
    [InlineData("multi", true)]
    [InlineData("MuLtI", true)]
    public void BeAsExpected(string name, bool expected)
    {
        var item = new AliasQueryResult { FileName = $"@{name}@" };

        item.IsComposite().ShouldBe(expected);
    }

    [Fact]
    public void BeMacroWhenSurroundedWithArobase()
    {
        var queryResult = new AliasQueryResult { FileName = "@name@" };
        queryResult.IsMacro().ShouldBeTrue();
    }

    [Theory]
    [InlineData("aze")]
    [InlineData("@aze")]
    [InlineData("aze@")]
    [InlineData("une/deux/trois/aze@")]
    [InlineData("une/deux/trois/@aze")]
    [InlineData(@"une\deux\trois\aze@")]
    [InlineData(@"une\deux\trois\@aze")]
    [InlineData(@"C:\Users\jibedoubleve\AppData\Local\Microsoft\TypeScript\4.4\node_modules\@types\sass-loader\node_modules\@types")]
    public void NotBeMacroWhenNotSurroundedWithArobase(string name)
    {
        var queryResult = new AliasQueryResult { FileName = name };
        queryResult.IsMacro().ShouldBeFalse();
    }

    #endregion
}