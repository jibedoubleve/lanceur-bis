using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Stores;
using Lanceur.SharedKernel.Mixins;
using Splat;

namespace Lanceur.Infra.Services
{
    public class SearchService : ISearchService
    {
        #region Fields

        private readonly IMacroManager _macroManager;
        private readonly IThumbnailManager _thumbnailManager;
        private readonly IStoreLoader _storeLoader;
        private IEnumerable<ISearchService> _stores;

        #endregion Fields

        #region Constructors

        public SearchService(IStoreLoader storeLoader = null, IMacroManager macroManager = null, IThumbnailManager thumbnailManager = null)
        {
            var l = Locator.Current;
            _storeLoader = storeLoader ?? new StoreLoader();
            _macroManager = macroManager ?? l.GetService<IMacroManager>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<ISearchService> Stores
        {
            get
            {
                if (_stores == null) { _stores = _storeLoader.Load(); }
                return _stores;
            }
        }

        #endregion Properties

        #region Methods

        public IEnumerable<QueryResult> GetAll()
        {
            var results = new List<QueryResult>();
            foreach (var store in Stores)
            {
                var res = store.GetAll();
                results.AddRange(res);
            }

            var toReturn = results?.Any() ?? false
                ? _macroManager.Handle(results)
                : results;
            _thumbnailManager.RefreshThumbnails(toReturn);
            return toReturn;
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            if (query == null) { return new List<QueryResult>(); }

            var results = new List<QueryResult>();
            foreach (var store in Stores)
            {
                var res = store.Search(query);
                results.AddRange(res);
            }

            foreach (var result in results) { result.Query = query; }

            if (results.Any())
            {
                // If there's an exact match, promote it to the top
                // of the list.
                var match = (from r in results
                             where r.Name == query.Name
                             select r).FirstOrDefault();
                if (match is not null) { results.Move(match, 0); }

                // Updgrade alias to executable macro and return the result
                var toReturn = _macroManager.Handle(results);
                _thumbnailManager.RefreshThumbnails(toReturn);
                return toReturn;
            }
            else { return DisplayQueryResult.SingleFromResult("No result found", iconKind: "AlertCircleOutline"); }
        }

        #endregion Methods
    }
}