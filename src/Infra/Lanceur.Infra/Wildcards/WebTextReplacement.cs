using System.Net;
using System.Text.RegularExpressions;

namespace Lanceur.Infra.Wildcards;

public class WebTextReplacement : IReplacement
{
    #region Fields

    private static readonly Regex Regex = new(@"\$[Ww]\$");

    #endregion Fields

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.Url;

    #endregion Properties

    #region Methods

    /// <inheritdoc />
    public string Replace(string text, string replacement)
    {
        replacement ??= string.Empty;
        text ??= string.Empty;

        var webParam = WebUtility.UrlEncode(replacement).ToLower();
        return Regex.Replace(text, webParam);
    }

    #endregion Methods
}