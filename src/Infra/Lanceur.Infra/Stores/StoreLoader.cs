using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using System.Reflection;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

public class StoreLoader : IStoreLoader
{
    private readonly IServiceProvider _serviceProvider;

    #region Fields

    private readonly ISearchServiceOrchestrator _orchestrator;
    private readonly ILogger<StoreLoader> _logger;
    private IEnumerable<IStoreService> _cachedStores;

    #endregion Fields

    #region Constructors
    
    public StoreLoader(ILogger<StoreLoader> logger, ISearchServiceOrchestrator orchestrator, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        
        _serviceProvider = serviceProvider;
        _logger = logger;
        _orchestrator = orchestrator;
    }

    #endregion Constructors

    #region Methods

    public IEnumerable<IStoreService> Load()
    {
        if (_cachedStores != null) return _cachedStores;

        using var _ = _logger.MeasureExecutionTime(this);
        var asm = Assembly.GetAssembly(typeof(SearchService));
        var types = asm?.GetTypes() ?? [];

        var found = types.Where(t => t.GetCustomAttributes(typeof(StoreAttribute)).Any())
                         .ToList();

        object[] args = [_serviceProvider];
        _cachedStores = found.Select(type => (IStoreService)Activator.CreateInstance(type, args)).ToArray();
        return _cachedStores;
    }

    #endregion Methods
}