using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using System.Reflection;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Infra.Stores
{
    public class StoreLoader : IStoreLoader
    {

        #region Fields

        private readonly ISearchServiceOrchestrator _orchestrator;
        private readonly ILogger<StoreLoader> _logger;
        private IEnumerable<ISearchService> _cachedStores;

        #endregion Fields

        #region Constructors

        public StoreLoader() : this(null)
        {
        }

        private StoreLoader(ILoggerFactory loggerFactory = null, ISearchServiceOrchestrator orchestrator = null)
        {
            loggerFactory ??= Locator.Current.GetService<ILoggerFactory>();
            _logger = loggerFactory.GetLogger<StoreLoader>();
            _orchestrator = orchestrator ?? Locator.Current.GetService<ISearchServiceOrchestrator>();
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<ISearchService> Load()
        {
            if (_cachedStores != null) return _cachedStores;
            
            using var _ = _logger.MeasureExecutionTime(this);
            var asm = Assembly.GetAssembly(typeof(SearchService));
            var types = asm.GetTypes();

            var found = (from t in types
                         where t.GetCustomAttributes(typeof(StoreAttribute)).Any()
                         select t).ToList();

            _cachedStores = found.Select(type => (ISearchService)Activator.CreateInstance(type))
                                 .ToArray();
            _orchestrator.Register(_cachedStores.ToArray());
            return _cachedStores;
        }

        #endregion Methods
    }
}