namespace Lanceur.Core.Managers;

public class StoreOrchestration
{
    #region Constructors

    private StoreOrchestration(string alivePattern, bool idleOthers)
    {
        AlivePattern = alivePattern;
        IdleOthers = idleOthers;
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// This is the regex to apply on the query to determine whether the search should be
    /// executed or not.
    /// </summary>
    /// <returns>
    /// The regex to apply to determine whether the service should be executed.
    /// </returns>
    public string AlivePattern { get; }

    /// <summary>
    /// This means the current store will silence all the other search
    /// </summary>
    /// <returns>
    /// <c>True</c> all other search won't occurs; otherwise <c>False</c>
    /// </returns>
    public bool IdleOthers { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Create an exclusive orchestrator, that's a store that will silence all the
    /// other search that can occurs at the same time
    /// </summary>
    /// <param name="alivePattern">The regex to apply to determine whether the service should be executed.</param>
    /// <returns>An Orchestrator</returns>
    public static StoreOrchestration Exclusive(string alivePattern) => new(alivePattern, true);

    /// <summary>
    /// Create an orchestrator that allows the store to run with all the
    /// other search that can occurs at the same time
    /// </summary>
    /// <param name="alivePattern">The regex to apply to determine whether the service should be executed.</param>
    /// <returns>An Orchestrator</returns>
    public static StoreOrchestration Shared(string alivePattern) => new(alivePattern, false);

    /// <summary>
    /// Create an orchestrator that allows the store to run with all the
    /// other search that can occurs at the same time. Furthermore this
    /// store will never be idle
    /// </summary>
    /// <returns>An Orchestrator</returns>
    public static StoreOrchestration SharedAlwaysActive() => new(string.Empty, false);

    #endregion Methods
}