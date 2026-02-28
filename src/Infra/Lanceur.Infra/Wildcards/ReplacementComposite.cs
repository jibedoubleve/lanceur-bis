using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Wildcards;

public class ReplacementComposite : IReplacement, IWildcardService
{
    #region Fields

    private readonly ILogger<ReplacementComposite> _logger;

    private readonly IEnumerable<IReplacement> _replacements;

    #endregion

    #region Constructors

    public ReplacementComposite(IClipboardService clipboard, ILogger<ReplacementComposite> logger)
    {
        _logger = logger;
        _replacements = new List<IReplacement>
        {
            new TextReplacement(),
            new WebTextReplacement(),
            new RawClipboardReplacement(clipboard),
            new WebClipboardReplacement(clipboard)
        };
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public string Wildcard => string.Empty;

    #endregion

    #region Methods

    /// <inheritdoc cref="IWildcardService" />
    public string Replace(string newText, string replacement)
    {
        var result = _replacements.Aggregate(
            newText,
            (current, text) => text.Replace(current, replacement)
        );

        _logger.LogTrace(
            "Replace text {Text} with {Replacement}. Replacement result: {Result}",
            newText,
            replacement.IsNullOrEmpty() ? replacement : "<EMPTY>",
            result
        );
        return result;
    }

    /// <inheritdoc />
    public string ReplaceOrReplacementOnNull(string text, string replacement)
        => text.IsNullOrWhiteSpace()
            ? replacement
            : Replace(text, replacement);

    #endregion
}