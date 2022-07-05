using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.Wildcards
{
    public class ReplacementCollection : IReplacement, IWildcardManager
    {
        #region Fields

        private readonly IEnumerable<IReplacement> _replacements;

        #endregion Fields

        #region Constructors

        public ReplacementCollection(IClipboardService clipboard)
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

        public string HandleArgument(string aliasParam, string userParam)
        {
            return aliasParam.IsNullOrWhiteSpace()
                ? userParam
                : Replace(aliasParam, userParam);
        }

        public string Replace(string text, string param)
        {
            foreach (var replacement in _replacements)
            {
                text = replacement.Replace(text, param);
            }

            return text;
        }

        #endregion Methods
    }
}