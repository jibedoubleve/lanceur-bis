using Lanceur.Core.Models;
using Lanceur.Core.Utils;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public class AliasQueryResultExtensionsTest
{
    #region Methods

    [Theory]
    [InlineData("steam://run/440",   440)]
    [InlineData("steam://run/12345", 12345)]
    [InlineData("steam://run/",      0)]   // No id → 0
    [InlineData(null,                0)]   // FileName null → 0
    public void When_getting_steam_id_Then_id_is_correct(string? fileName, int expected)
    {
        // ARRANGE
        var alias = new AliasQueryResult { FileName = fileName };

        // ACT
        var result = alias.GetSteamId();

        // ASSERT
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("steam://run/440",    true)]
    [InlineData("steam://run/12345",  true)]
    [InlineData("steam://run/",       false)]
    [InlineData("https://google.com", false)]
    [InlineData("C:\\app.exe",        false)]
    [InlineData(null,                 false)]
    public void When_checking_is_steam_game_Then_result_is_correct(string? fileName, bool expected)
    {
        // ARRANGE
        var alias = new AliasQueryResult { FileName = fileName };

        // ACT
        var result = alias.IsSteamGame();

        // ASSERT
        result.ShouldBe(expected);
    }

    #endregion
}