using System.ComponentModel;
using System.Web.Bookmarks;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.ReservedAliases;

[ReservedAlias("clrbm")]
[Description("Clears or invalidates the bookmark cache to ensure fresh data.")]
public class ClearBookmarkCacheAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IBookmarkRepository _bookmarks;

    private readonly ILogger<ClearBookmarkCacheAlias> _logger;
    private readonly IConfigurationFacade _configuration;

    #endregion

    #region Constructors

    public ClearBookmarkCacheAlias(
        ILoggerFactory loggerFactory,
        IBookmarkRepositoryFactory bookmarkRepositoryFactory,
        IConfigurationFacade configuration)
    {
        _configuration = configuration;
        _bookmarks = bookmarkRepositoryFactory.BuildBookmarkRepository(
            configuration.Application.Stores.BookmarkSourceBrowser
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
            _configuration.Application.Stores.BookmarkSourceBrowser
        );

        _bookmarks.InvalidateCache();
        return NoResultAsync;
    }

    #endregion
}