using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Stores;

public abstract class StoreBase
{
    #region Constructors

    protected StoreBase(IStoreOrchestrationFactory orchestrationFactory)
        => StoreOrchestrationFactory
            = orchestrationFactory ??
              throw new ArgumentException(
                  $"The {typeof(IStoreOrchestrationFactory)} should be configured in the IOC container."
              );

    #endregion

    #region Properties

    protected IStoreOrchestrationFactory StoreOrchestrationFactory { get; }

    #endregion

    #region Methods

    public virtual IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    #endregion
}