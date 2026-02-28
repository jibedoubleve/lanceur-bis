using System.Net;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class FavIconDownloader : IFavIconDownloader
{
    #region Fields

    private readonly HttpClient _client;
    private readonly IMemoryCache _faviconCache;
    private readonly ILogger<FavIconDownloader> _logger;
    private readonly TimeSpan _retryDelay;

    private static readonly Dictionary<string, (bool IsManual, string Url)> FaviconUrls = new()
    {
        ["DuckDuckGo"] = (IsManual: false, Url: "https://icons.duckduckgo.com/ip2/{0}.ico"),
        ["Google"] = (IsManual: false, Url: "https://www.google.com/s2/favicons?domain_url={0}"),
        ["Manual (png)"] = (IsManual: true, Url: "favicon.png"),
        ["Manual (ico)"] = (IsManual: true, Url: "favicon.ico")
    };

    #endregion

    #region Constructors

    public FavIconDownloader(
        ILogger<FavIconDownloader> logger,
        IMemoryCache faviconCache,
        TimeSpan retryDelay,
        IHttpClientFactory httpClientFactory
    )
    {
        _logger = logger;
        _faviconCache = faviconCache;
        _retryDelay = retryDelay;
        _client = httpClientFactory.CreateClient("favicon");
    }

    #endregion

    #region Methods

    private string  GetFaviconUrl(Uri url, KeyValuePair<string, (bool IsManual, string Url)> manager)
    {
        var requestUrl = manager.Value.IsManual
            ? new Uri(url, manager.Value.Url).ToString()
            : manager.Value.Url.Format(url.Host);

        _logger.LogTrace("Request Url: {Url}", requestUrl);
        return requestUrl;
    }

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
                _logger.LogInformation("Cannot save thumbnail, response from favicon url is empty.");
                return false;
            }

            await File.WriteAllBytesAsync(outputPath, bytes);
            foundFavIcon = true;
            return true;
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to save favicon into {OutputPath}.", outputPath); }

        return foundFavIcon;
    }

    public async Task<bool> RetrieveAndSaveFavicon(Uri url, string outputPath)
    {
        if (_faviconCache.TryGetValue(url.ToString(), out _))
        {
            _logger.LogTrace("{Url} is in the failed paths for favicon retrieving.", url);
            return false;
        }

        foreach (var faviconUrl in FaviconUrls)
            try
            {
                var requestUrl = GetFaviconUrl(url, faviconUrl);
                var response = await _client.SendAsync(new(HttpMethod.Get, requestUrl));

                _logger.LogTrace(
                    "Checking favicon with {FavIconManager} - Status: {Status} - Host: {Host}",
                    faviconUrl.Key,
                    response.StatusCode,
                    url.Host
                );
                if (response.StatusCode != HttpStatusCode.OK) { continue; }

                if (!await SaveThumbnailAsync(response, outputPath)) { continue; }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(
                    ex,
                    "Error while retrieving favicon for {Url} - {Manager}",
                    url,
                    faviconUrl.Value.Url
                );
            }

        if (!_faviconCache.TryGetValue(url.ToString(), out _)) { _faviconCache.Set(url.ToString(), true, _retryDelay); }

        return false;
    }

    #endregion
}