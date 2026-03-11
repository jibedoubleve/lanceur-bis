using System.Text.RegularExpressions;
using Lanceur.Core.Models;

namespace Lanceur.Core.Utils;

public static partial class AliasQueryResultExtensions
{
    #region Methods

    [GeneratedRegex(@"steam://run/(\d+)")]
    private static partial Regex SteamGameRegex();

    public static int GetSteamId(this AliasQueryResult alias)
    {
        if (alias.FileName is null) { return 0; }

        var match = SteamGameRegex().Match(alias.FileName);
        return match.Success 
               && int.TryParse(match.Groups[1].Value, out var id) ? id : 0;
    }

    public static bool IsSteamGame(this AliasQueryResult alias)
        => alias.FileName != null
           && SteamGameRegex().IsMatch(alias.FileName!);

    #endregion
}