using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public sealed class AliasQueryResultTest
{
    #region Methods

    [Fact]
    public void When_checking_IsPackagedApplication_Then_no_exception_is_thrown()
    {
        // ARRANGE
        var alias = new AliasQueryResult();

        // ACT
        var action = () => alias.IsPackagedApplication();

        // ASSERT
        action.ShouldNotThrow();
    }

    /// <summary>
    ///     Regression test for issue #1175. Check the sanitisation can be run against null.
    /// </summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData(@"C:\dir\dir2\file.exe", @"C:\dir\dir2\file.exe")]
    [InlineData(@"""C:\dir\dir2\file.exe""", @"C:\dir\dir2\file.exe")]
    public void When_sanitize_FileName_Then_quotes_are_removed(string filename, string expected)
    {
        // ARRANGE
        var alias = new AliasQueryResult { FileName = filename };

        // ACT
        alias.SanitizeFileName();

        // ASSERT
        alias.FileName.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("a", "a")]
    [InlineData("a, b", "a,b")]
    [InlineData(" a , b ", "a,b")]
    [InlineData("a,,b,", "a,b")]
    [InlineData("a b", "a,b")]
    [InlineData("a  b", "a,b")]
    [InlineData("a, b c", "a,b,c")]
    public void When_sanitize_Synonyms_Then_commas_and_spaces_split_names(string? synonyms, string? expected)
    {
        // ARRANGE
        var alias = new AliasQueryResult { Synonyms = synonyms };

        // ACT
        alias.SanitizeSynonyms();

        // ASSERT
        alias.Synonyms.ShouldBe(expected);
    }

    #endregion
}