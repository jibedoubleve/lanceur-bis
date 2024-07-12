using System.Text.RegularExpressions;

namespace Lanceur.Infra.Wildcards
{
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
        public string Replace(string text, string replacement)
        {
            text ??= string.Empty;
            replacement ??= string.Empty;

            return Regex.Replace(text, replacement);
        }

        #endregion Methods
    }
}