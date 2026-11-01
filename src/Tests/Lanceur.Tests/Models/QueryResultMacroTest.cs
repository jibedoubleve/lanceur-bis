using Lanceur.Core.Models;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public sealed class QueryResultMacroTest
{
    #region Methods

    [Theory]
    [InlineData("aze")]
    [InlineData("@aze")]
    [InlineData("aze@")]
    [InlineData("une/deux/trois/aze@")]
    [InlineData("une/deux/trois/@aze")]
    [InlineData(@"une\deux\trois\aze@")]
    [InlineData(@"une\deux\trois\@aze")]
    [InlineData(
        @"C:\Users\jibedoubleve\AppData\Local\Microsoft\TypeScript\4.4\node_modules\@types\sass-loader\node_modules\@types"
    )]
    public void When_file_name_is_not_surrounded_with_arobase_Then_it_is_not_macro(string name)
    {
        var queryResult = new AliasQueryResult { FileName = name };
        queryResult.IsMacro().ShouldBeFalse();
    }

    [Fact]
    public void When_file_name_is_surrounded_with_arobase_Then_it_is_macro()
    {
        var queryResult = new AliasQueryResult { FileName = "@name@" };
        queryResult.IsMacro().ShouldBeTrue();
    }

    #endregion
}