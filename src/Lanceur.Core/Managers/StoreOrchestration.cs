using System.Text.RegularExpressions;

namespace Lanceur.Core.Managers;

public sealed class StoreOrchestration
{
    #region Constructors

    internal StoreOrchestration(Regex alivePattern, bool idleOthers)
    {
        AlivePattern = alivePattern;
        IdleOthers = idleOthers;
    }

    internal StoreOrchestration(string alivePattern, bool idleOthers)
    {
        AlivePattern = AsRegex(alivePattern);
        IdleOthers = idleOthers;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     This is the regex to apply on the query to determine whether the search should be
    ///     executed or not.
    /// </summary>
    /// <returns>
    ///     The regex to apply to determine whether the service should be executed.
    /// </returns>
    public Regex AlivePattern { get; }

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
            $@"(^\s*){pattern}(.*)",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200)
        );

    #endregion
}