using System.Web.Bookmarks;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Stores;

[Store]
public class BookmarksStore : IStoreService
{
    #region Fields

    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public BookmarksStore(IServiceProvider serviceProvider)
    {
        _settings = serviceProvider.GetService<ISettingsFacade>();
        var memoryCache = serviceProvider.GetService<IMemoryCache>();
        _bookmarkRepository = new ChromeBookmarksRepository(memoryCache);
    }

    #endregion

    #region Properties

    public StoreOrchestration StoreOrchestration => StoreOrchestration.Exclusive(@"^\s{0,}/.*");

    #endregion

    #region Methods

    public IEnumerable<QueryResult> GetAll() => _bookmarkRepository.GetBookmarks()
                                                                   .Select(
                                                                       e => new AliasQueryResult { Name = e.Name, FileName = e.Url }
                                                                   );

    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        var r  = _bookmarkRepository.GetBookmarks(query.Parameters)
                                    .Select(
                                        e => new AliasQueryResult { Name = e.Name, FileName = e.Url }
                                    );
        return r;
    }

    #endregion
}