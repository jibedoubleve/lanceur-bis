﻿using System.IO;
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

    /// <inheritdoc />
    public async Task UpdateThumbnailAsync(QueryResult queryResult)
    {
        if (queryResult.IsThumbnailDisabled) return;
        if (queryResult is not AliasQueryResult alias) return;

        if (alias.FileName.IsNullOrEmpty())
        {
            _logger.LogTrace("Skipping thumbnail for alias {Name} because FileName is null or empty.", alias.Name);
            return;
        }

        if (File.Exists(alias.Thumbnail))
        {
            _logger.LogTrace("Alias {AliasName} already has a thumbnail", alias.Name);
            return;
        }

        var filePath = alias.FileName.GetThumbnailPath();
        if (File.Exists(filePath))
        {
            if (alias.Thumbnail == filePath) return;

            _logger.LogTrace("Thumbnail already exists for {AliasName} but not yet updated. (PackagedApp)", alias.Name);
            alias.Thumbnail = filePath;
            queryResult.MarkChanged();
            return;
        }

        _logger.LogTrace("Loading thumbnail for {AliasName}...", alias.Name);

        // ----
        // Manage packaged applications
        // ----
        if (alias.IsPackagedApplication())
        {
            var app = await _packagedAppSearchService.GetByInstalledDirectoryAsync(alias.FileName);
            var response = app.FirstOrDefault();

            if (response is not null)
            {
                alias.Thumbnail = response.Logo.LocalPath;
                queryResult.MarkChanged();
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
            queryResult.MarkChanged();
            return;
        }

        // ----
        // Manage URL
        // ----
        if (!alias.FileName.IsUrl())
        {
            _logger.LogTrace(
                "Skipping thumbnail for alias {Name} because {FileName} is not an URL.",
                alias.Name,
                alias.FileName
            );
            return;
        }

        var favicon = alias.FileName
                           .GetKeyForFavIcon()
                           .GetThumbnailPath();

        if (File.Exists(favicon))
        {
            alias.Thumbnail = favicon;
            queryResult.MarkChanged();
            return;
        }

        // Fire & forget...
        _ = _favIconService.UpdateFaviconAsync(alias);
    }

    /// <summary>
    ///     Starts a thread to refresh the thumbnails asynchronously. This method returns immediately after starting the
    ///     thread.
    ///     Each time a thumbnail is found, the corresponding alias is updated. Because the alias is reactive, the UI will
    ///     automatically reflect these updates.
    /// </summary>
    /// <param name="queries">The list of queries for which thumbnails need to be updated.</param>
    public void UpdateThumbnails(IEnumerable<QueryResult> queries)
    {
        queries = queries.ToArray();


        using var m = _logger.WarnIfSlow(this);
        try
        {
            _logger.LogTrace("Refreshing thumbnails for {Count} alias", queries.Count());
            foreach (var query in queries)
                UpdateThumbnailAsync(query) // Fire & forget thumbnail refresh
                    .ContinueWith(
                        t =>
                        {
                            _logger.LogWarning(
                                t.Exception!,
                                "An error occured when updating thumbnail: {Message}",
                                t.Exception!.Message
                            );
                        },
                        TaskContinuationOptions.OnlyOnFaulted
                    );
            _logger.LogTrace("Fire and forget the refresh of thumbnails.");
        }
        catch (Exception ex) { _logger.LogWarning(ex, "An error occured during the refresh of the icons"); }
    }

    #endregion
}