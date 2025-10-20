using System.Web.Bookmarks;
using Lanceur.Core.Configuration;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Extensions;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Stores;

[Store]
public class BookmarksStore : Store, IStoreService
{
    #region Fields

    private readonly IBookmarkRepositoryFactory _bookmarkRepositoryFactory;
    private readonly ISection<StoreSection> _settings;

    #endregion

    #region Constructors

    public BookmarksStore(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _settings = serviceProvider.GetSection<StoreSection>();
        _bookmarkRepositoryFactory = serviceProvider.GetService<IBookmarkRepositoryFactory>();
        serviceProvider.GetService<ILogger<BookmarksStore>>();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => true;

    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Exclusive(@"^\s{0,}/.*");

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        var bookmarkSourceBrowser = _settings.Value.BookmarkSourceBrowser;
        var repository = _bookmarkRepositoryFactory.BuildBookmarkRepository(bookmarkSourceBrowser);

        if (!repository.IsBookmarkSourceAvailable())
            return DisplayQueryResult.SingleFromResult("The bookmark source is not available!");

        if (cmdline.Parameters.IsNullOrWhiteSpace())
            return DisplayQueryResult.SingleFromResult("Enter text to search in your browser's bookmarks...");

        return repository.GetBookmarks(cmdline.Parameters)
                         .Select(e => e.ToAliasQueryResult())
                         .ToList();
    }

    #endregion
}