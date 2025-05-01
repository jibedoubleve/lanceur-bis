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
        _logger.LogTrace("Before wildcard replacement: {Text}", replacement);
        
        newText = _replacements.Aggregate(
            newText, 
            (current, text) => text.Replace(current, replacement)
        );

        _logger.LogTrace("After wildcard replacement: {Text}", newText);
        return newText;
    }

    /// <inheritdoc />
    public string ReplaceOrReplacementOnNull(string text, string replacement) => text.IsNullOrWhiteSpace()
        ? replacement
        : Replace(text, replacement);

    #endregion
}