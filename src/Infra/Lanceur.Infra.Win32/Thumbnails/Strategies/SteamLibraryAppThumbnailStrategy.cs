using System.IO;
using System.Net;
using System.Net.Http;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Win32.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails.Strategies;

public class SteamLibraryAppThumbnailStrategy : ThumbnailStrategy
{
    #region Fields

    private readonly IAliasManagementService _aliasManagementService;

    private readonly IFavIconHttpClient _httpClient;
    private readonly ILogger<SteamLibraryAppThumbnailStrategy> _logger;

    #endregion

    #region Constructors

    public SteamLibraryAppThumbnailStrategy(
        IFavIconHttpClient httpClient,
        ILogger<SteamLibraryAppThumbnailStrategy> logger,
        IAliasManagementService aliasManagementService) : base(logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _aliasManagementService = aliasManagementService;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public override int Priority => 300;

    #endregion

    #region Methods

    private async Task<bool> SaveThumbnailAsync(HttpResponseMessage response, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(outputPath);

        var foundFavIcon = false;

        try
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0)
            {
                _logger.LogInformation("Cannot save thumbnail, response from Steam url is empty.");
                return false;
            }

            await File.WriteAllBytesAsync(outputPath, bytes);
            foundFavIcon = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save Steam thumbnail into {OutputPath}.", outputPath);
        }

        return foundFavIcon;
    }

    /// <inheritdoc />
    protected override async Task UpdateThumbnailCoreAsync(AliasQueryResult alias, CancellationToken cancellationToken)
    {
        if (!alias.IsSteamGame()) { return; }

        var url = GetThumbnailUrl(alias.GetSteamId());
        using var response =
            await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);

        _logger.LogTrace(
            "Checking steam thumbnail - Status: {Status} - Host: {Host}",
            response.StatusCode,
            url
        );

        if (response.StatusCode != HttpStatusCode.OK) { return; }

        var thumbnail = alias.ResolveThumbnailAbsolutePath();

        if (!File.Exists(thumbnail) && !await SaveThumbnailAsync(response, thumbnail)) { return; }

        alias.Thumbnail = thumbnail;
        _aliasManagementService.UpdateThumbnail(alias);

        return;

        string GetThumbnailUrl(int appId) =>
            $"https://cdn.akamai.steamstatic.com/steam/apps/{appId}/capsule_sm_120.jpg";
    }

    #endregion
}