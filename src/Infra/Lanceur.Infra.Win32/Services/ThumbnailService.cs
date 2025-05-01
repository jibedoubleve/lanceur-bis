using System.IO;
using Lanceur.Core.Decorators;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Images;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public class ThumbnailService : IThumbnailService
{
    #region Fields

    private readonly IAliasRepository _aliasRepository;
    private readonly IFavIconService _favIconService;
    private readonly ILogger<ThumbnailService> _logger;
    private readonly IPackagedAppSearchService _packagedAppSearchService;
    private readonly Win32ThumbnailService _win32ThumbnailService;

    #endregion

    #region Constructors

    public ThumbnailService(
        ILoggerFactory loggerFactory,
        IAliasRepository aliasRepository,
        IPackagedAppSearchService packagedAppSearchService,
        IFavIconService favIconService
    )
    {
        _win32ThumbnailService = new(loggerFactory.CreateLogger<Win32ThumbnailService>());
        _aliasRepository = aliasRepository;
        _packagedAppSearchService = packagedAppSearchService;
        _favIconService = favIconService;
        _logger = loggerFactory.GetLogger<ThumbnailService>();
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Updates the thumbnail for the provided query. This method handles different types of sources:
    ///     executables, Windows Store applications, and URLs. It attempts to retrieve and assign the appropriate
    ///     thumbnail or favicon based on the query information.
    /// </summary>
    /// <param name="queryResult">An object containing the necessary information to retrieve and update the thumbnail.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task UpdateThumbnailAsync(EntityDecorator<QueryResult> queryResult)
    {
        if (queryResult.Entity?.IsThumbnailDisabled ?? true) return;
        if (queryResult.Entity is not AliasQueryResult alias) return;
        if (alias.FileName.IsNullOrEmpty()) return;
        if (File.Exists(alias.Thumbnail))  return; 

        // ----
        // Manage packaged applications
        // ----
        var filePath = alias.FileName.GetThumbnailPath();
        if (File.Exists(filePath)) { alias.Thumbnail = filePath; }
        else if (alias.IsPackagedApplication())
        {
            var response = (await _packagedAppSearchService.GetByInstalledDirectoryAsync(alias.FileName))
                .FirstOrDefault();
            if (response is not null)
            {
                alias.Thumbnail = response.Logo.LocalPath;
                queryResult.Soil();
            }

            alias.Thumbnail.CopyToImageRepository(alias.FileName);
            return;
        }

        // ----
        // Manage win32 applications
        // ----
        var imageSource = _win32ThumbnailService.GetThumbnail(alias.FileName);
        if (imageSource is not null)
        {
            var file = new FileInfo(alias.FileName);
            imageSource.CopyToImageRepository(file.Name);
            alias.Thumbnail = file.Name.GetThumbnailPath();
            queryResult.Soil();
            return;
        }

        // ----
        // Manage URL
        // ----
        if (!alias.FileName.IsUrl())
        {
            _logger.LogTrace("Skipping thumbnail for alias {Name} because {FileName} is not an URL.", alias.Name, alias.FileName);
            return;
        }

        var favicon = alias.FileName
                           .GetKeyForFavIcon()
                           .GetThumbnailPath();

        if (File.Exists(favicon))
        {
            alias.Thumbnail = favicon;
            queryResult.Soil();
            return;
        }

        await _favIconService.RetrieveFaviconAsync(alias);
    }

    /// <summary>
    ///     Starts a thread to refresh the thumbnails asynchronously. This method returns immediately after starting the
    ///     thread.
    ///     Each time a thumbnail is found, the corresponding alias is updated. Because the alias is reactive, the UI will
    ///     automatically reflect these updates.
    /// </summary>
    /// <param name="queryResults">The list of queries for which thumbnails need to be updated.</param>
    public void UpdateThumbnails(IEnumerable<QueryResult> queryResults)
    {
        queryResults = queryResults.ToArray();
        var queries = EntityDecorator<QueryResult>.FromEnumerable(queryResults)
                                                  .ToArray();

        using var m = _logger.WarnIfSlow(this);
        try
        {
            _logger.LogTrace("Refreshing thumbnails for {Count} alias", queryResults.Count());
            foreach (var query in queries)
                UpdateThumbnailAsync(query) // Fire & forget thumbnail refresh
                    .ContinueWith(
                        t =>
                        {
                            _logger.LogWarning(t.Exception!, "An error occured when updating thumbnail: {Message}", t.Exception!.Message);
                        }, TaskContinuationOptions.OnlyOnFaulted);
            _logger.LogTrace("Fire and forget the refresh of thumbnails.");
        }
        catch (Exception ex) { _logger.LogWarning(ex, "An error occured during the refresh of the icons"); }
    }

    #endregion
}