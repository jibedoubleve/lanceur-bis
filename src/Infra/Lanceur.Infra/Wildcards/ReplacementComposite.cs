using System.Collections.Generic;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Wildcards;

public class ReplacementComposite : IReplacement, IWildcardService
{
    #region Fields

    private readonly IEnumerable<IReplacement> _replacements;

    #endregion Fields

    #region Constructors

    public ReplacementComposite(IClipboardService clipboard) => _replacements = new List<IReplacement>() { new TextReplacement(), new WebTextReplacement(), new RawClipboardReplacement(clipboard), new WebClipboardReplacement(clipboard) };

    #endregion Constructors

    #region Properties

    /// <inheritdoc />
    public string Wildcard => string.Empty;

    #endregion Properties

    #region Methods

    /// <inheritdoc cref="IWildcardService"/>
    public string Replace(string text, string withThis)
    {
        foreach (var replacement in _replacements) text = replacement.Replace(text, withThis);

        return text;
    }

    /// <inheritdoc />
    public string ReplaceOrReplacementOnNull(string text, string replacement) => text.IsNullOrWhiteSpace()
        ? replacement
        : Replace(text, replacement);

    #endregion Methods
}