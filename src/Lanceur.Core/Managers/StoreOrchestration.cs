namespace Lanceur.Core.Managers;

public class StoreOrchestration
{
    #region Constructors

    internal StoreOrchestration(string alivePattern, bool idleOthers)
    {
        AlivePattern = alivePattern;
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
    public string AlivePattern { get; }

    /// <summary>
    ///     This means the current store will silence all the other search
    /// </summary>
    /// <returns>
    ///     <c>True</c> all other search won't occur; otherwise <c>False</c>
    /// </returns>
    public bool IdleOthers { get; }

    #endregion
}