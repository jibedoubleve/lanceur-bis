using Lanceur.Core.Services;
using System.Net;

namespace Lanceur.Infra.Wildcards
{
    /// <summary>
    /// Replace with the text in the clipboard as it is.
    /// No work on the text is done
    /// </summary>
    public class WebClipboardReplacement : IReplacement
    {
        #region Fields

        private readonly IClipboardService _clipboard;

        #endregion Fields

        #region Constructors

        public WebClipboardReplacement(IClipboardService clipboard)
        {
            _clipboard = clipboard;
        }

        #endregion Constructors

        #region Properties

        public string Wildcard => Wildcards.WebClipboard;

        #endregion Properties

        #region Methods

        public string Replace(string text, string param)
        {
            var clipboard = _clipboard.GetText();
            clipboard = WebUtility.UrlEncode(clipboard ?? "")?.ToLower() ?? "";

            return text
                ?.Replace(Wildcard.ToLower(), clipboard)
                ?.Replace(Wildcard.ToUpper(), clipboard)
                ?? "";
        }

        #endregion Methods
    }
}