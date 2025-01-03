using System.Collections.Concurrent;
using Windows.ApplicationModel;

namespace Lanceur.Infra.Win32.PackagedApp;

public static class PackageExtensions
{
    #region Fields

    private static readonly ConcurrentDictionary<string, string> CacheAppUser = new();

    private static readonly ConcurrentDictionary<string, bool> CacheIsAppUser = new();

    #endregion Fields

    #region Methods

    public static string GetAppUserModelId(this Package entry)
    {
        var key = entry.Id.FullName;

        if (CacheAppUser.TryGetValue(key, out var value)) return value;

        var result = entry.GetAppListEntries()
                          .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                          .Select(e => e.AppUserModelId)
                          .FirstOrDefault(e => !string.IsNullOrEmpty(e));
        CacheAppUser[key] = result ?? string.Empty;

        return CacheAppUser[key];
    }

    public static bool IsAppUserModelId(this Package entry, string fileName)
    {
        var key = $"{fileName}-{entry.Id.FullName}";
        if (CacheIsAppUser.TryGetValue(key, out var value)) return value;

        var result = entry.GetAppListEntries()
                          .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                          .Select(e => e.AppUserModelId)
                          .FirstOrDefault(e => !string.IsNullOrEmpty(e) && e == fileName);
        CacheIsAppUser[key] = result is not null;

        return CacheIsAppUser[key];
    }

    #endregion Methods
}