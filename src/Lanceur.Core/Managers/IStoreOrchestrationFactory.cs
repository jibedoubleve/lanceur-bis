namespace Lanceur.Core.Managers;

public interface IStoreOrchestrationFactory
{
    #region Methods

    /// <summary>
    ///     Create an exclusive orchestrator, that's a store that will silence all the
    ///     other search that can occur at the same time
    /// </summary>
    /// <param name="alivePattern">The regex to apply to determine whether the service should be executed.</param>
    /// <returns>An Orchestrator</returns>
    StoreOrchestration Exclusive(string alivePattern);

    /// <summary>
    ///     Create an orchestrator that allows the store to run with all the
    ///     other search that can occurs at the same time
    /// </summary>
    /// <param name="alivePattern">The regex to apply to determine whether the service should be executed.</param>
    /// <returns>An Orchestrator</returns>
    StoreOrchestration Shared(string alivePattern);

    /// <summary>
    ///     Create an orchestrator that allows the store to run with all the
    ///     other search that can occur at the same time. Furthermore, this
    ///     store will never be idle
    /// </summary>
    /// <returns>An Orchestrator</returns>
    StoreOrchestration SharedAlwaysActive();

    #endregion
}