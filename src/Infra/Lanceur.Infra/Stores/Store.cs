using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Stores;

public abstract class Store
{
    #region Constructors

    protected Store(IStoreOrchestrationFactory factory)
    {
        StoreOrchestrationFactory
            = factory ??
              throw new NullReferenceException(
                  $"The {typeof(IStoreOrchestrationFactory)} should be configured in the IOC container."
              );
    }

    #endregion

    #region Properties

    protected IStoreOrchestrationFactory StoreOrchestrationFactory { get; }

    #endregion

    #region Methods

    public virtual IEnumerable<QueryResult> GetAll() => QueryResult.NoResult;

    #endregion
}