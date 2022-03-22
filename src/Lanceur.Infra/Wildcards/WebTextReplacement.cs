using System.Net;

namespace Lanceur.Infra.Wildcards
{
    public class WebTextReplacement : IReplacement
    {
        #region Properties

        public string Wildcard => Wildcards.Url;

        #endregion Properties

        #region Methods

        public string Replace(string text, string param)
        {
            var p = WebUtility.UrlEncode(param ?? "")?.ToLower() ?? "";
            return text
                ?.Replace(Wildcard.ToLower(), p)
                ?.Replace(Wildcard.ToUpper(), p)
                ?? "";
        }

        #endregion Methods
    }
}