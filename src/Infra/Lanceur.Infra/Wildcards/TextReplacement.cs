using System.Text.RegularExpressions;

namespace Lanceur.Infra.Wildcards;

public partial class TextReplacement : IReplacement
{
    #region Fields

    private static readonly Regex Regex = GetRegex();

    #endregion

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.Text;

    #endregion

    #region Methods

    [GeneratedRegex(@"\$[Ii]\$")] private static partial Regex GetRegex();

    /// <inheritdoc />
    public string Replace(string newText, string replacement)
    {
        newText ??= string.Empty;
        replacement ??= string.Empty;

        return Regex.Replace(newText, replacement);
    }

    #endregion
}