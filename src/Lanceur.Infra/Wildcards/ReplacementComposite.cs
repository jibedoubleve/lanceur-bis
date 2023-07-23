using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Wildcards
{
    public class ReplacementComposite : IReplacement, IWildcardManager
    {
        #region Fields

        private readonly IEnumerable<IReplacement> _replacements;

        #endregion Fields

        #region Constructors

        public ReplacementComposite(IClipboardService clipboard)
        {
            _replacements = new List<IReplacement>()
            {
                new TextReplacement(),
                new WebTextReplacement(),
                new RawClipboardReplacement(clipboard),
                new WebClipboardReplacement(clipboard)
            };
        }

        #endregion Constructors

        #region Properties

        public string Wildcard => string.Empty;

        #endregion Properties

        #region Methods

        /// <inheritdoc />
        public string ReplaceOrReplacementOnNull(string text, string withThis)
        {
            return text.IsNullOrWhiteSpace()
                ? withThis
                : Replace(text, withThis);
        }

        /// <inheritdoc />
        public string Replace(string text, string withThis)
        {
            foreach (var replacement in _replacements)
            {
                text = replacement.Replace(text, withThis);
            }

            return text;
        }

        #endregion Methods
    }
}