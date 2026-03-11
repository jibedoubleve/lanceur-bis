using Lanceur.Core.Managers;
using Lanceur.Core.Models;

namespace Lanceur.Infra.Stores;

[Obsolete("Should be renamed as StoreBase to avoid issue with StoreAttribute")]
public abstract class Store
{
    #region Constructors

    protected Store(IStoreOrchestrationFactory orchestrationFactory)
        => StoreOrchestrationFactory
            = orchestrationFactory ??
              throw new NullReferenceException(
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