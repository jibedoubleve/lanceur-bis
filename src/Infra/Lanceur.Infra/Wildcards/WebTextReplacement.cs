using System.Net;
using System.Text.RegularExpressions;

namespace Lanceur.Infra.Wildcards;

public partial class WebTextReplacement : IReplacement
{
    #region Fields

    private static readonly Regex Regex = GetRegex();

    #endregion

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.Url;

    #endregion

    #region Methods

    [GeneratedRegex(@"\$[Ww]\$")] private static partial Regex GetRegex();

    /// <inheritdoc />
    public string Replace(string newText, string replacement)
    {
        replacement ??= string.Empty;
        newText ??= string.Empty;

        var webParam = WebUtility.UrlEncode(replacement).ToLower();
        return Regex.Replace(newText, webParam);
    }

    #endregion
}