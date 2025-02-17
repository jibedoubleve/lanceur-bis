using System.Net;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.SharedKernel.Web;

public class FavIconDownloader2 : IFavIconDownloader
{
    #region Fields

    private readonly HttpClient _client = new();

    /// <summary>
    ///     Cache the path where it is impossible to retrieve the favicon to not spend time
    ///     on something that doesn't work
    /// </summary>
    private readonly HashSet<string> _failedPaths = [];

    private readonly ILogger<FavIconDownloader2> _logger;

    private static readonly Dictionary<string, (bool IsManual, string Url)> FavIconManagers = new()
    {
        ["Manual_png"] = (IsManual: true, Url: "favicon.png"),
        ["Manual_ico"] = (IsManual: true, Url: "favicon.ico"),
        ["DuckDuckGo"] = (IsManual: false, Url: "https://icons.duckduckgo.com/ip2/{0}.ico"),
        ["Google"] = (IsManual: false, Url: "https://www.google.com/s2/favicons?domain_url={0}")
    };

    #endregion

    #region Constructors

    public FavIconDownloader2(ILogger<FavIconDownloader2> logger) => _logger = logger;

    #endregion

    #region Methods

    private string  RequestUrl(Uri url, KeyValuePair<string, (bool IsManual, string Url)> manager)
    {
        var requestUrl = manager.Value.IsManual
            ? new Uri(url, manager.Value.Url).ToString()
            : manager.Value.Url.Format(url.Host);

        _logger.LogTrace("Request Url: {Url}", requestUrl);
        return requestUrl;
    }

    public async Task<bool> CheckExistsAsync(Uri url)
    {
        foreach (var manager in FavIconManagers)
            try
            {
                var requestUrl = RequestUrl(url, manager);
                var result = await _client.SendAsync(new(HttpMethod.Get, requestUrl));

                _logger.LogTrace(
                    "Checking favicon with {FavIconManager} - Status: {Status} - Host: {Host}",
                    manager.Key,
                    result.StatusCode,
                    url.Host
                );
                if (result.StatusCode == HttpStatusCode.OK) return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Error while retrieving favicon for {Url} - {Manager}",
                    url,
                    manager.Value.Url
                );
                _failedPaths.Add(url.AbsoluteUri);
            }

        return false;
    }

    public async Task<bool> SaveToFileAsync(Uri url, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(outputPath);

        if (_failedPaths.TryGetValue(url.AbsoluteUri, out _)) return false;

        var foundFavIcon = false;

        var tasks = new List<Task>();
        foreach (var favIconUrl in FavIconManagers.Select(manager => RequestUrl(url, manager)))
            try
            {
                var bytes = await _client.GetByteArrayAsync(favIconUrl);
                if (bytes.Length == 0)
                {
                    _logger.LogInformation("Failed to save favicon to {Url} with {FavIconUrl}", url, favIconUrl);
                    continue;
                }

                tasks.Add(File.WriteAllBytesAsync(outputPath, bytes));
                foundFavIcon = true;
                break;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    ex,
                    "Failed to retrieve favicon for {Url} with {FavIconUrl}",
                    url,
                    favIconUrl
                );
            }

        await Task.WhenAll(tasks);

        return foundFavIcon;
    }

    #endregion
}