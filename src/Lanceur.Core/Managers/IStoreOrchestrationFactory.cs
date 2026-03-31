using System.Text.RegularExpressions;

namespace Lanceur.Core.Managers;

public interface IStoreOrchestrationFactory
{
    #region Methods

    /// <summary>
    ///     Creates an orchestrator that permanently idles the store regardless of the query.
    ///     Use this to disable a store at runtime (e.g. when a feature flag is off).
    /// </summary>
    /// <returns>An orchestrator that never activates the store.</returns>
    StoreOrchestration AlwaysInactive();

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
    StoreOrchestration Shared(Regex alivePattern);

    /// <summary>
    ///     Creates a shared orchestrator that evaluates <paramref name="alivePattern" /> against the
    ///     full command line (name + parameters) rather than the command name alone.
    ///     Use this for stores whose activation regex must inspect the entire expression,
    ///     such as a calculator where operands appear after the first token.
    /// </summary>
    /// <param name="alivePattern">The regex applied to the full cmdline string to determine whether the store should be activated.</param>
    /// <returns>An orchestrator that does not silence other stores.</returns>
    StoreOrchestration SharedOnFullQuery(Regex alivePattern);

    /// <summary>
    ///     Create an orchestrator that allows the store to run with all the
    ///     other search that can occur at the same time. 
    /// </summary>
    /// <returns>An Orchestrator</returns>
    StoreOrchestration SharedAlwaysActive();

    #endregion
}