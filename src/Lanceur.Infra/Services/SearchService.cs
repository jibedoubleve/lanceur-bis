using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Infra.Services
{
    public class SearchService : SearchServiceCache, ISearchService
    {
        #region Fields

        private readonly IMacroManager _macroManager;
        private readonly IThumbnailManager _thumbnailManager;
        private readonly ILogger<SearchService> _logger;

        #endregion Fields

        #region Constructors

        public SearchService(
            IStoreLoader storeLoader = null, 
            IMacroManager macroManager = null, 
            IThumbnailManager thumbnailManager = null,
            ILoggerFactory loggerFactory = null) : base(storeLoader)
        {
            var l = Locator.Current;
            _macroManager = macroManager ?? l.GetService<IMacroManager>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();

            loggerFactory ??= l.GetService<ILoggerFactory>();
            _logger = loggerFactory.GetLogger<SearchService>();
        }

        #endregion Constructors

        #region Methods

        private IEnumerable<QueryResult> SetupAndSort(QueryResult[] input)
        {
            // Upgrade alias to executable macro and return the result
            var results = input?.Any() ?? false
                ? _macroManager.Handle(input).ToList()
                : new();

            // Refresh the thumbnails
            _thumbnailManager.RefreshThumbnailsAsync(results);

            // Order the list and return the result
            var orderedResults = results.OrderByDescending(e => e.Count)
                                        .ThenBy(e => e.Name)
                                        .ToArray();

            return input?.Any() ?? false
                ? orderedResults
                : DisplayQueryResult.NoResultFound;
        }

        public IEnumerable<QueryResult> GetAll()
        {
            var results = Stores.SelectMany(store => store.GetAll())
                                .ToArray();

            return SetupAndSort(results);
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            using var _ = _logger.MeasureExecutionTime(this);
            if (query == null) { return new List<QueryResult>(); }

            var results = Stores
                .SelectMany(store => store.Search(query))
                .ToArray();

            // Remember the query
            foreach (var result in results) { result.Query = query; }

            // If there's an exact match, promote it to the top
            // of the list.
            var orderedResults = SetupAndSort(results).ToList();
            var match = (from r in orderedResults
                         where r.Name == query.Name
                         select r).FirstOrDefault();
            if (match is not null) { orderedResults.Move(match, 0); }

            return !orderedResults.Any()
                ? DisplayQueryResult.SingleFromResult("No result found", iconKind: "AlertCircleOutline")
                : orderedResults;
        }

        #endregion Methods
    }
}