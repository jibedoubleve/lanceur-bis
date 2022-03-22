using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Splat;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class AliasStore : ISearchService
    {
        #region Fields

        private readonly IDataService _aliasService;

        #endregion Fields

        #region Constructors

        public AliasStore() : this(null)
        {
        }

        public AliasStore(IDataService aliasService)
        {
            _aliasService = aliasService ?? Locator.Current.GetService<IDataService>();
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<QueryResult> GetAll() => _aliasService.GetAll();

        public IEnumerable<QueryResult> Search(Cmdline query) => _aliasService.Search(query.Name);

        #endregion Methods
    }
}