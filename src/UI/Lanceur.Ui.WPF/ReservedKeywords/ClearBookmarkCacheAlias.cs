using System.ComponentModel;
using System.Web.Bookmarks;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.ReservedKeywords;

[ReservedAlias("clrbm")]
[Description("Clears or invalidates the bookmark cache to ensure fresh data.")]
public class ClearBookmarkCacheAlias : SelfExecutableQueryResult
{
    #region Fields

    private readonly IBookmarkRepository _bookmarks;

    private readonly ILogger<ClearBookmarkCacheAlias> _logger;
    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public ClearBookmarkCacheAlias(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var factory = serviceProvider.GetService<ILoggerFactory>() ??
                      throw new NullReferenceException("Logger factory is ont configured in the service provider");

        var bookmarkRepositoryFactory
            = serviceProvider.GetService<IBookmarkRepositoryFactory>() ??
              throw new NullReferenceException(
                  "Bookmark repository is not configured in the service provider"
              );

        _settings = serviceProvider.GetService<ISettingsFacade>() ??
                    throw new NullReferenceException("Settings facade is not configured in the service provider");

        _bookmarks = bookmarkRepositoryFactory.BuildBookmarkRepository(
            _settings.Application.Stores.BookmarkSourceBrowser
        );
        _logger = factory.CreateLogger<ClearBookmarkCacheAlias>();
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
            _settings.Application.Stores.BookmarkSourceBrowser
        );

        _bookmarks.InvalidateCache();
        return NoResultAsync;
    }

    #endregion
}