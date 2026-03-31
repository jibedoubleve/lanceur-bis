using System.Text.RegularExpressions;

namespace Lanceur.Core.Managers;

/// <inheritdoc />
public sealed class StoreOrchestrationFactory : IStoreOrchestrationFactory
{
    #region Methods

    /// <inheritdoc />
    public StoreOrchestration AlwaysInactive() => new("(?!)", false);

    /// <inheritdoc />
    public StoreOrchestration Exclusive(string alivePattern) => new(alivePattern, true);

    /// <inheritdoc />
    public StoreOrchestration Shared(Regex alivePattern) => new(alivePattern, false);

    /// <inheritdoc />
    public StoreOrchestration SharedOnFullQuery(Regex alivePattern) => new(alivePattern, false, c => c.ToString());

    /// <inheritdoc />
    public StoreOrchestration SharedAlwaysActive() => new(string.Empty, false);

    #endregion
}