using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public class AliasQueryResultTest
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

    #endregion
}