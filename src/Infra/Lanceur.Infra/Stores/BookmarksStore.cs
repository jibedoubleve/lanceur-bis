using System.Web.Bookmarks;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Extensions;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Stores;

[Store(@"^\s{0,}/.*")]
public class BookmarksStore : StoreBase, IStoreService
{
    #region Fields

    private readonly IBookmarkRepositoryFactory _bookmarkRepositoryFactory;

    #endregion

    #region Constructors

    public BookmarksStore(
        IStoreOrchestrationFactory orchestrationFactory,
        IBookmarkRepositoryFactory bookmarkRepositoryFactory,
        ISection<StoreSection> storeSettings
    ) : base(orchestrationFactory, storeSettings)
    {
        _bookmarkRepositoryFactory = bookmarkRepositoryFactory;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool IsOverridable => true;

    public StoreOrchestration StoreOrchestration => StoreOrchestrationFactory.Exclusive(DefaultShortcut);

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        var bookmarkSourceBrowser = StoreSettings.Value.BookmarkSourceBrowser;
        var repository = _bookmarkRepositoryFactory.BuildBookmarkRepository(bookmarkSourceBrowser);

        if (!repository.IsBookmarkSourceAvailable())
        {
            return DisplayQueryResult.SingleFromResult("The bookmark source is not available!");
        }

        if (cmdline.Parameters.IsNullOrWhiteSpace())
        {
            return DisplayQueryResult.SingleFromResult("Enter text to search in your browser's bookmarks...");
        }

        return repository.GetBookmarks(cmdline.Parameters)
                         .Select(e => e.ToAliasQueryResult())
                         .ToList();
    }

    #endregion
}