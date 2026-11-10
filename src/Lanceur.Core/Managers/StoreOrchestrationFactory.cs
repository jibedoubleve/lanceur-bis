using System.Text.RegularExpressions;

namespace Lanceur.Core.Managers;

/// <inheritdoc />
public sealed partial class StoreOrchestrationFactory : IStoreOrchestrationFactory
{
    #region Methods

    /// <remarks>
    ///     <c>(?:)</c> is an empty group: it matches at position 0 of any input without consuming characters.
    /// </remarks>
    [GeneratedRegex("(?:)")]
    private static partial Regex AlwaysMatchRegex();

    /// <summary>
    ///     Returns a regex that never matches any input, including an empty string.
    ///     Used by <see cref="AlwaysInactive" /> to ensure the store is never activated.
    /// </summary>
    /// <remarks>
    ///     <c>(?!)</c> is an empty negative lookahead: an empty pattern always matches,
    ///     so its negation fails at every position.
    /// </remarks>
    [GeneratedRegex("(?!)")]
    private static partial Regex NeverMatchRegex();

    /// <inheritdoc />
    public StoreOrchestration AlwaysInactive() => new(NeverMatchRegex(), false);

    /// <inheritdoc />
    public StoreOrchestration Exclusive(string shortcut) => new(StoreOrchestration.ExactMatch(shortcut), true);

    /// <inheritdoc />
    public StoreOrchestration Shared(Regex alivePattern) => new(alivePattern, false);

    /// <inheritdoc />
    public StoreOrchestration SharedAlwaysActive() => new(AlwaysMatchRegex(), false);

    /// <inheritdoc />
    public StoreOrchestration SharedOnFullQuery(Regex alivePattern) => new(alivePattern, false, c => c.ToString());

    #endregion
}