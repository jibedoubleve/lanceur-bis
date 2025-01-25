using System.Web.Bookmarks;
using Humanizer;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Stores;

[Store]
public class BookmarksStore : IStoreService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;

    private readonly IBookmarkRepositoryFactory _bookmarkRepositoryFactory;
    private readonly ISettingsFacade _settings;

    private const int Length = 66;

    #endregion

    #region Constructors

    public BookmarksStore(IServiceProvider serviceProvider)
    {
        _settings = serviceProvider.GetService<ISettingsFacade>();
        _bookmarkRepositoryFactory = serviceProvider.GetService<IBookmarkRepositoryFactory>();
        _aliasRepository = serviceProvider.GetService<IAliasRepository>();
    }

    #endregion

    #region Properties

    public StoreOrchestration StoreOrchestration => StoreOrchestration.Exclusive(@"^\s{0,}/.*");

    #endregion

    #region Methods


    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll()
    {
        var bookmarks = _bookmarkRepositoryFactory.CreateBookmarkRepository(_settings.Application.Stores.BookmarkSourceBrowser)
                                                  .GetBookmarks()
                                                  .Select(
                                                      e => new AliasQueryResult { Name = e.Name.Truncate(Length, "(...)"), FileName = e.Url, Count = -1}
                                                  )
                                                  .ToList();
        return bookmarks;
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline query)
    {
        if (query.Parameters.IsNullOrWhiteSpace())
        {
            return DisplayQueryResult.SingleFromResult("Enter text to search in your browser's bookmarks...");
        }
        
        var bookmarks  = _bookmarkRepositoryFactory.CreateBookmarkRepository(_settings.Application.Stores.BookmarkSourceBrowser)
                                                   .GetBookmarks(query.Parameters)
                                                   .Select(
                                                       e => new AliasQueryResult { Name = e.Name.Truncate(Length, "(...)"), FileName = e.Url, Count = -1}
                                                   )
                                                   .ToList();
        return bookmarks;
    }

    #endregion
}