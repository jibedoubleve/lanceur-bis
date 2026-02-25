using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public class AliasQueryResultTest
{
    #region Methods

    /// <summary>
    ///     Regression test for issue #1175. Check the sanitisation can be run against null.
    /// </summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData(@"C:\dir\dir2\file.exe", @"C:\dir\dir2\file.exe")]
    [InlineData(@"""C:\dir\dir2\file.exe""", @"C:\dir\dir2\file.exe")]
    public void When_sanitize_FileName_Then_quotes_are_removed(string filename, string expected)
    {
        // Arrange
        var alias = new AliasQueryResult { FileName = filename };

        // Act
        alias.SanitizeFileName();

        // Assert
        alias.FileName.ShouldBe(expected);
    }

    #endregion
}