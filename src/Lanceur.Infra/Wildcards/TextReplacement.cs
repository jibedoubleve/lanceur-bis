namespace Lanceur.Infra.Wildcards
{
    public class TextReplacement : IReplacement
    {
        #region Properties

        public string Wildcard => Wildcards.Text;

        #endregion Properties

        #region Methods

        public string Replace(string text, string param)
        {
            return text
                    ?.Replace(Wildcard.ToLower(), param ?? "")
                    ?.Replace(Wildcard.ToUpper(), param ?? "")
                    ?? "";
        }

        #endregion Methods
    }
}