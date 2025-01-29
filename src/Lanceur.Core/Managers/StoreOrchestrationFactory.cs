namespace Lanceur.Core.Managers;

/// <inheritdoc />
public class StoreOrchestrationFactory : IStoreOrchestrationFactory
{
    #region Methods

    /// <inheritdoc />
    public StoreOrchestration Exclusive(string alivePattern) => new(alivePattern, true);

    /// <inheritdoc />
    public StoreOrchestration Shared(string alivePattern) => new(alivePattern, false);

    /// <inheritdoc />
    public StoreOrchestration SharedAlwaysActive() => new(string.Empty, false);

    #endregion
}