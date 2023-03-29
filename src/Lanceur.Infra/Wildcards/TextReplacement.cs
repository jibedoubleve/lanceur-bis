namespace Lanceur.Infra.Wildcards
{
    public class TextReplacement : IReplacement
    {
        #region Properties

        public string Wildcard => Wildcards.Text;

        #endregion Properties

        #region Methods

        public string Replace(string text, string with)
        {
            return text
                    ?.Replace(Wildcard.ToLower(), with ?? "")
                    ?.Replace(Wildcard.ToUpper(), with ?? "")
                    ?? "";
        }

        #endregion Methods
    }
}