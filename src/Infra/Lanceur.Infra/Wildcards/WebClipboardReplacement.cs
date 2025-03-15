using Lanceur.Core.Services;
using System.Net;
using System.Text.RegularExpressions;

namespace Lanceur.Infra.Wildcards;

/// <summary>
/// Replace with the text in the clipboard as it is.
/// No work on the text is done
/// </summary>
public class WebClipboardReplacement : IReplacement
{
    #region Fields

    private static readonly Regex Regex = new(@"\$[Cc]\$");

    private readonly IClipboardService _clipboard;

    #endregion Fields

    #region Constructors

    public WebClipboardReplacement(IClipboardService clipboard) => _clipboard = clipboard;

    #endregion Constructors

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.WebClipboard;

    #endregion Properties

    #region Methods

    /// <inheritdoc />
    public string Replace(string text, string replacement)
    {
        var clipboard = _clipboard.RetrieveText() ?? string.Empty;
        clipboard = WebUtility.UrlEncode(clipboard).ToLower();

        text ??= string.Empty;

        return Regex.Replace(text, clipboard);
    }

    #endregion Methods
}