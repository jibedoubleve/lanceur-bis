using System.Text.RegularExpressions;

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

    #endregion

    #region Constructors

    internal StoreOrchestration(string alivePattern, bool idleOthers)
    {
        _aliveRegex = AsRegex(alivePattern);
        IdleOthers = idleOthers;
    }

    internal StoreOrchestration(Regex aliveRegex, bool idleOthers)
    {
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

    public bool IsMatch(string query) => _aliveRegex.IsMatch(query);

    #endregion
}