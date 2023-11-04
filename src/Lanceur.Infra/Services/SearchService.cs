using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Mixins;
using Splat;

namespace Lanceur.Infra.Services
{
    public class SearchService : SearchServiceCache, ISearchService
    {
        #region Fields

        private readonly IMacroManager _macroManager;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public SearchService(IStoreLoader storeLoader = null, IMacroManager macroManager = null, IThumbnailManager thumbnailManager = null) : base(storeLoader)
        {
            var l = Locator.Current;
            _macroManager = macroManager ?? l.GetService<IMacroManager>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
        }

        #endregion Constructors

        #region Methods

        private IEnumerable<QueryResult> SetupAndSort(List<QueryResult> results)
        {
            // Upgrade alias to executable macro and return the result
            var toReturn = results?.Any() ?? false
                ? _macroManager.Handle(results).ToList()
                : new();

            // Refresh the thumbnails
            _thumbnailManager.RefreshThumbnails(toReturn);

            // Order the list and return the result
            var result = toReturn.OrderByDescending(e => e.Count)
                                 .ThenBy(e => e.Name)
                                 .ToArray();
            
            return results?.Any() ?? false 
                ? result 
                : DisplayQueryResult.NoResultFound;
        }

        public IEnumerable<QueryResult> GetAll()
        {
            var results = Stores
                .SelectMany(store => store.GetAll())
                .ToList();

            return SetupAndSort(results);
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            if (query == null) { return new List<QueryResult>(); }

            var results = Stores
                .SelectMany(store => store.Search(query))
                .ToList();


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