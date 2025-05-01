using System.Text.RegularExpressions;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Wildcards;

/// <summary>
/// Replace with the text in the clipboard as it is.
/// No work on the text is done
/// </summary>
public class RawClipboardReplacement : IReplacement
{
    private static readonly Regex Regex = new(@"\$[rR]\$");

    #region Fields

    private readonly IClipboardService _clipboard;

    #endregion Fields

    #region Constructors

    public RawClipboardReplacement(IClipboardService clipboard) => _clipboard = clipboard;

    #endregion Constructors

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.RawClipboard;

    #endregion Properties

    #region Methods

    /// <inheritdoc/>
    public string Replace(string newText, string replacement)
    {
        var clipboard = _clipboard.RetrieveText() ?? string.Empty;
        newText ??= string.Empty;

        return Regex.Replace(newText, clipboard);
    }

    #endregion Methods
}