using Lanceur.Core.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Stores;

public abstract class Store
{
    #region Constructors

    public Store(IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetService<IStoreOrchestrationFactory>();

        StoreOrchestrationFactory = factory ?? throw new NullReferenceException($"The {typeof(IStoreOrchestrationFactory)} should be configured in the IOC container.");
    }

    #endregion

    #region Properties

    protected IStoreOrchestrationFactory StoreOrchestrationFactory { get; }

    #endregion
}