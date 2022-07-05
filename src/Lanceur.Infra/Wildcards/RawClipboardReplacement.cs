using Lanceur.Core.Services;

namespace Lanceur.Infra.Wildcards
{
    /// <summary>
    /// Replace with the text in the clipboard as it is.
    /// No work on the text is done
    /// </summary>
    public class RawClipboardReplacement : IReplacement
    {
        #region Fields

        private readonly IClipboardService _clipboard;

        #endregion Fields

        #region Constructors

        public RawClipboardReplacement(IClipboardService clipboard)
        {
            _clipboard = clipboard;
        }

        #endregion Constructors

        #region Properties

        public string Wildcard => Wildcards.RawClipboard;

        #endregion Properties

        #region Methods

        public string Replace(string text, string param)
        {
            var clipboard = _clipboard.GetText();
            return text
                    ?.Replace(Wildcard.ToLower(), clipboard ?? "")
                    ?.Replace(Wildcard.ToUpper(), clipboard ?? "")
                    ?? "";
        }

        #endregion Methods
    }
}