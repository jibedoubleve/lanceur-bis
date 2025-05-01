using System.Text.RegularExpressions;

namespace Lanceur.Infra.Wildcards;

public class TextReplacement : IReplacement
{
    #region Fields

    private static readonly Regex Regex = new(@"\$[Ii]\$");

    #endregion Fields

    #region Properties

    /// <inheritdoc />
    public string Wildcard => Wildcards.Text;

    #endregion Properties

    #region Methods

    /// <inheritdoc />
    public string Replace(string newText, string replacement)
    {
        newText ??= string.Empty;
        replacement ??= string.Empty;

        return Regex.Replace(newText, replacement);
    }

    #endregion Methods
}