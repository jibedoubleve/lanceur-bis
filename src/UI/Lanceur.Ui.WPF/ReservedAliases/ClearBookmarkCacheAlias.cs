using System.ComponentModel;
using System.Web.Bookmarks;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("clrbm")]
[Description("Clears or invalidates the bookmark cache to ensure fresh data.")]
public sealed class ClearBookmarkCacheAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IBookmarkRepository _bookmarks;
    private readonly ILogger<ClearBookmarkCacheAlias> _logger;
    private readonly ISection<StoreSection> _storeSection;

    #endregion

    #region Constructors

    public ClearBookmarkCacheAlias(
        ILoggerFactory loggerFactory,
        IBookmarkRepositoryFactory bookmarkRepositoryFactory,
        ISection<StoreSection> storeSection
    )
    {
        _storeSection = storeSection;
        _bookmarks = bookmarkRepositoryFactory.BuildBookmarkRepository(
            storeSection.Value.BookmarkSourceBrowser
        );
        _logger = loggerFactory.CreateLogger<ClearBookmarkCacheAlias>();
    }

    #endregion

    #region Properties

    public override string Icon => "TextClearFormatting24";

    #endregion

    #region Methods

    public override Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null)
    {
        _logger.LogInformation(
            "Clearing bookmarks cache for {Browser}",
            _storeSection.Value.BookmarkSourceBrowser
        );

        _bookmarks.InvalidateCache();
        return NoResultAsync;
    }

    #endregion
}