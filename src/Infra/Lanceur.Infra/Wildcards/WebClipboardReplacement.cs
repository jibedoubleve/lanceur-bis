using System.Net;
using System.Text.RegularExpressions;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Wildcards;

/// <summary>
///     Replace with the text in the clipboard as it is.
///     No work on the text is done
/// </summary>
public partial class WebClipboardReplacement : IReplacement
{
    #region Fields

    private readonly IClipboardService _clipboard;

    private static readonly Regex Regex = GetRegex();

    #endregion

    #region Constructors

    public WebClipboardReplacement(IClipboardService clipboard) => _clipboard = clipboard;

    #endregion

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.WebClipboard;

    #endregion

    #region Methods

    [GeneratedRegex(@"\$[Cc]\$")] private static partial Regex GetRegex();

    /// <inheritdoc />
    public string Replace(string newText, string replacement)
    {
        var clipboard = _clipboard.RetrieveText() ?? string.Empty;
        clipboard = WebUtility.UrlEncode(clipboard).ToLower();

        newText ??= string.Empty;

        return Regex.Replace(newText, clipboard);
    }

    #endregion
}