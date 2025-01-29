using System.Reflection;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

public class StoreLoader : IStoreLoader
{
    #region Fields

    private readonly ILogger<StoreLoader> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISettingsFacade _settings;
    private const string CacheKey = $"CachedStore_{nameof(StoreLoader)}";

    #endregion

    #region Constructors

    public StoreLoader(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()
                                 .GetLogger<StoreLoader>();
        _memoryCache = serviceProvider.GetService<IMemoryCache>();
        _settings = serviceProvider.GetService<ISettingsFacade>();
    }

    #endregion

    #region Methods

    private void UpdateOverrides(IStoreService[] stores)
    {
        if (_settings is null) return;
        
        _settings.Reload();
        var settingsOverrides = _settings.Application.Stores?.StoreOverrides ?? [];
        
        // Get all unsaved overrides and add them to the settings
        var overrides
            = stores.Where(
                store =>
                {
                    var storeOverrides = (_settings.Application.Stores?.StoreOverrides ?? []).ToArray();
                    
                    // If no store shortcut override is defined in the settings, simply check whether it can be overridden.
                    if (!storeOverrides.Any()) return !store.StoreOrchestration.AlivePattern.IsNullOrWhiteSpace() && store.IsOverridable;
                    
                    // Overriden shortcut exists, check whether this shortcut is not already in the settings
                    // and otherwise, check whether it can be overriden
                    return !store.StoreOrchestration.AlivePattern.IsNullOrWhiteSpace() 
                           && store.IsOverridable 
                           && storeOverrides.All(e => e.StoreType != store.GetType()); 
                })
                    .Select(store => new StoreShortcut { AliasOverride = store.StoreOrchestration.AlivePattern, StoreType = store.GetType() })
                    .Where(storeOverride => settingsOverrides.All(x => x.StoreType != storeOverride.StoreType))
                    .ToList();
        
        _settings.Application.Stores!.StoreOverrides = settingsOverrides.Concat(overrides);
        _settings.Save();
    }

    public IEnumerable<IStoreService> Load()
    {
        using var _ = _logger.MeasureExecutionTime(this);
        return _memoryCache.GetOrCreate(
            CacheKey,
            IEnumerable<IStoreService> (_) =>
            {
                var asm = Assembly.GetAssembly(typeof(SearchService));
                var types = asm?.GetTypes() ?? [];

                var found = types.Where(t => t.GetCustomAttributes(typeof(StoreAttribute)).Any())
                                 .ToList();

                object[] args = [_serviceProvider];
                var stores = found.Select(type => (IStoreService)Activator.CreateInstance(type, args)).ToArray();
                UpdateOverrides(stores);
                return stores;
            }
        );
    }

    #endregion
}