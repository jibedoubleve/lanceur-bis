using System.Web.Bookmarks;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Extensions;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Lanceur.Infra.Stores;

[Store]
public class BookmarksStore : Store, IStoreService
{
    #region Fields

    private readonly IBookmarkRepositoryFactory _bookmarkRepositoryFactory;
    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public BookmarksStore(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _settings = serviceProvider.GetService<ISettingsFacade>();
        _bookmarkRepositoryFactory = serviceProvider.GetService<IBookmarkRepositoryFactory>();
        serviceProvider.GetService<IAliasRepository>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => true;

    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Exclusive(@"^\s{0,}/.*");

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> GetAll()
    {
        var repository = _bookmarkRepositoryFactory.BuildBookmarkRepository(_settings.Application.Stores.BookmarkSourceBrowser);
        if (!repository.IsBookmarkSourceAvailable()) { return DisplayQueryResult.SingleFromResult("The bookmark source is not available!"); }

        return repository.GetBookmarks()
                         .Select(e => e.ToAliasQueryResult())
                         .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        var repository = _bookmarkRepositoryFactory.BuildBookmarkRepository(_settings.Application.Stores.BookmarkSourceBrowser);
        if (!repository.IsBookmarkSourceAvailable()) { return DisplayQueryResult.SingleFromResult("The bookmark source is not available!"); }

        if (cmdline.Parameters.IsNullOrWhiteSpace()) return DisplayQueryResult.SingleFromResult("Enter text to search in your browser's bookmarks...");

        return repository. GetBookmarks(cmdline.Parameters)
                         .Select(e => e.ToAliasQueryResult())
                         .ToList();
    }

    #endregion
}