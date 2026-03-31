using System.Text.RegularExpressions;
using Lanceur.Core.Models;

namespace Lanceur.Core.Managers;

public sealed class StoreOrchestration
{
    #region Fields

    /// <summary>
    ///     This is the regex to apply on the query to determine whether the search should be
    ///     executed or not.
    /// </summary>
    /// <returns>
    ///     The regex to apply to determine whether the service should be executed.
    /// </returns>
    private readonly Regex _aliveRegex;

    private readonly Func<Cmdline, string> _selector;

    #endregion

    #region Constructors

    internal StoreOrchestration(string alivePattern, bool idleOthers, Func<Cmdline, string>? selector = null)
        : this(AsRegex(alivePattern), idleOthers, selector) { }

    internal StoreOrchestration(Regex aliveRegex, bool idleOthers, Func<Cmdline, string>? selector = null)
    {
        _selector = selector ?? (c => c.Name);
        _aliveRegex = aliveRegex;
        IdleOthers = idleOthers;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     This means the current store will silence all the other search
    /// </summary>
    /// <returns>
    ///     <c>True</c> all other search won't occur; otherwise <c>False</c>
    /// </returns>
    public bool IdleOthers { get; }

    #endregion

    #region Methods

    private static Regex AsRegex(string pattern)
        => new(
            $@"(^\s*){Regex.Escape(pattern)}(.*)",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200)
        );

    /// <summary>
    ///     Determines whether this store should handle the given query by matching
    ///     the alive pattern against <see cref="Cmdline.Name" /> only — parameters are intentionally excluded
    ///     to prevent false positives when a parameter value happens to contain the store's trigger character.
    /// </summary>
    /// <param name="query">The parsed command line whose <see cref="Cmdline.Name" /> is evaluated.</param>
    /// <returns><c>true</c> if the store should be activated for this query; otherwise <c>false</c>.</returns>
    public bool IsAlive(Cmdline query) 
        => _aliveRegex.IsMatch(_selector(query));

    #endregion
}