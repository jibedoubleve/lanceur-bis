using System.IO;
using System.Text.RegularExpressions;
using Humanizer;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace Lanceur.Infra.Win32.Services;

public partial class SteamLibraryService : ISteamLibraryService
{
    #region Fields

    private DateTime _lastCacheUpdate = DateTime.MinValue;

    private readonly ILogger<SteamLibraryService> _logger;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKeyGames = "SteamLibraryService_Games";
    private const string CacheKeyLibrary = "SteamLibraryService_Libraries";
    private const string SteamApps = "steamapps";

    #endregion

    #region Constructors

    public SteamLibraryService(IMemoryCache memoryCache, ILogger<SteamLibraryService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    #endregion

    #region Methods

    [GeneratedRegex(@"""appid""\s+""(\d+)""")]
    private static partial Regex AppIdPattern();

    [GeneratedRegex(@"""name""\s+""(.+?)""")]
    private static partial Regex GameNamePattern();

    private static IEnumerable<SteamGame> GetGames(string repoPath)
    {
        var games = new List<SteamGame>();

        if (!Directory.Exists(repoPath))
        {
            return [];
        }

        foreach (var file in Directory.GetFiles(repoPath, "appmanifest_*.acf"))
        {
            var content = File.ReadAllText(file);
            var appid = AppIdPattern().Match(content);
            var name = GameNamePattern().Match(content);

            if (appid.Success && name.Success)
            {
                games.Add(new SteamGame(
                    int.Parse(appid.Groups[1].Value),
                    name.Groups[1].Value)
                );
            }
        }

        return games;
    }

    private IEnumerable<string> GetLibraryPaths()
    {
        var steamPath = GetSteamInstallationPath();
        var vdf = Path.Combine(steamPath, SteamApps, "libraryfolders.vdf");

        if (!File.Exists(vdf)) { return []; }

        return _memoryCache.GetOrCreate(CacheKeyLibrary,
            entry => {
                entry.AbsoluteExpirationRelativeToNow = 10.Seconds();
                var libraries = new List<string>();
                
                foreach (Match m in KeyValuePattern().Matches(File.ReadAllText(vdf)))
                {
                    _logger.LogDebug("Found library path: {LibraryPath}", m.Groups[1].Value);
                    libraries.Add(Path.Combine(m.Groups[1].Value.Replace(@"\\", @"\"), SteamApps));
                }

                return libraries;
            }) ?? [];
    }

    private static string GetSteamInstallationPath()
    {
        if (Registry.CurrentUser
                    .OpenSubKey(@"SOFTWARE\Valve\Steam")
                    ?.GetValue("SteamPath") is string regPath) { return regPath; }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam");
    }

    [GeneratedRegex(@"""path""\s+""(.+?)""")]
    private static partial Regex KeyValuePattern();

    /// <inheritdoc />
    public IEnumerable<SteamGame> GetGames()
    {
        var libraries = GetLibraryPaths().ToArray();

        var invalidateCache = libraries.Any(path => {
            var lastWrite = Directory.GetLastWriteTime(path);
            return lastWrite > _lastCacheUpdate;
        });
        if (invalidateCache)
        {
            _lastCacheUpdate = DateTime.Now;
            _memoryCache.Remove(CacheKeyGames);
        }

        return _memoryCache.GetOrCreate(CacheKeyGames,
            _ => {
                var games = new List<SteamGame>();
                foreach (var repoPath in libraries)
                {
                    games.AddRange(GetGames(repoPath));
                }

                return games;
            }) ?? Enumerable.Empty<SteamGame>();
    }

    #endregion
}