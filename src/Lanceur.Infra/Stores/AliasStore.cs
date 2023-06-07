using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Splat;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class AliasStore : ISearchService
    {
        #region Fields

        private readonly IDbRepository _aliasService;

        #endregion Fields

        #region Constructors

        public AliasStore() : this(null)
        {
        }

        public AliasStore(IDbRepository aliasService)
        {
            _aliasService = aliasService ?? Locator.Current.GetService<IDbRepository>();
        }

        #endregion Constructors

        #region Methods

        public IEnumerable<QueryResult> GetAll() => _aliasService.GetAll();

        public IEnumerable<QueryResult> Search(Cmdline query) => _aliasService.Search(query.Name);

        #endregion Methods
    }
}