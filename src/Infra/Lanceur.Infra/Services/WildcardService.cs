using System.Net;
using System.Text.RegularExpressions;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public partial class WildcardService : IWildcardService
{
    #region Fields

    private readonly IClipboardService _clipboard;

    #endregion

    #region Constructors

    public WildcardService(IClipboardService clipboard) => _clipboard = clipboard;

    #endregion

    #region Methods

    [GeneratedRegex(@"\$([a-zA-Z])\$")]
    private static partial Regex Wildcard();

    /// <inheritdoc />
    public bool HasWildcard(string? template) =>
        template is not null && Wildcard().IsMatch(template);

    /// <inheritdoc />
    public string Replace(string? template, string? input)
    {
        template ??= string.Empty;
        input ??= string.Empty;

        return Wildcard().Replace(template,
            m => m.Groups[1].Value.ToUpperInvariant() switch
            {
                "I" => input,
                "R" => _clipboard.RetrieveText(),
                "W" => WebUtility.UrlEncode(input),
                "C" => WebUtility.UrlEncode(_clipboard.RetrieveText()),
                _ => throw new NotSupportedException(
                    $"Wildcard template ${m.Groups[1].Value.ToUpperInvariant()}$ is not supported")
            });
    }

    #endregion
}