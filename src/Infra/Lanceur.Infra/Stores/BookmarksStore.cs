using System.Web.Bookmarks;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Extensions;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Infra.Stores;

[Store("/")]
public sealed class BookmarksStore : StoreBase, IStoreService
{
    #region Fields

    private readonly IBookmarkRepositoryFactory _bookmarkRepositoryFactory;

    #endregion

    #region Constructors

    public BookmarksStore(
        IStoreOrchestrationFactory orchestrationFactory,
        IBookmarkRepositoryFactory bookmarkRepositoryFactory,
        ISection<StoreSection> storeSettings
    ) : base(orchestrationFactory, storeSettings) =>
        _bookmarkRepositoryFactory = bookmarkRepositoryFactory;

    #endregion

    #region Properties

    /// <inheritdoc cref="IStoreService.IsOverridable" />
    public override bool IsOverridable => true;

    public StoreOrchestration Orchestration => StoreOrchestrationFactory.Exclusive(Shortcut);

    #endregion

    #region Methods

    private static bool IsRefinementOf(string value, string check)
        => check.Contains(value, StringComparison.InvariantCultureIgnoreCase);

    private static bool IsRefinementOf(QueryResult query, string value)
        => IsRefinementOf(value, query.Name);

    private static string SelectProperty(Cmdline cmdline) => cmdline.Parameters;

    /// <inheritdoc cref="CanPruneResult" />
    public override bool CanPruneResult(Cmdline previous, Cmdline current)
    {
        if (SelectProperty(previous).Length == 0) { return false; }

        return OverrideCanPruneResult(
            previous,
            current,
            SelectProperty,
            IsRefinementOf
        );
    }

    /// <inheritdoc cref="PruneResult" />
    public override int PruneResult(IList<QueryResult> destination, Cmdline previous, Cmdline current)
        => OverridePruneResult(
            destination,
            previous,
            current,
            SelectProperty,
            IsRefinementOf
        );

    /// <inheritdoc />
    public IEnumerable<QueryResult> Search(Cmdline cmdline)
    {
        var bookmarkSourceBrowser = StoreSettings.Value.BookmarkSourceBrowser;
        var repository = _bookmarkRepositoryFactory.BuildBookmarkRepository(bookmarkSourceBrowser);

        var query = SelectProperty(cmdline);

        if (!repository.IsBookmarkSourceAvailable())
        {
            return DisplayQueryResult.SingleFromResult("The bookmark source is not available!");
        }

        if (query.IsNullOrWhiteSpace())
        {
            return DisplayQueryResult.SingleFromResult("Enter text to search in your browser's bookmarks...");
        }

        return repository.GetBookmarks(query)
                         .Select(e => e.ToAliasQueryResult())
                         .ToList();
    }

    #endregion
}