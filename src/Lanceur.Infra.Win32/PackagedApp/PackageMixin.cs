using System.Collections.Concurrent;
using Windows.ApplicationModel;

namespace Lanceur.Infra.Win32.PackagedApp;

public static class PackageMixin
{
    #region Fields

    private static readonly ConcurrentDictionary<string, string> Cache = new();
    
    private static readonly ConcurrentDictionary<string, bool> Cache_ = new();

    #endregion Fields

    #region Methods

    public static bool IsAppUserModelId(this Package entry, string fileName)
    {
        var key = $"{fileName}-{entry.Id.FullName}";
        if (Cache_.TryGetValue(key, out var value)) return value;

        var result = entry.GetAppListEntries()
                          .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                          .Select(e => e.AppUserModelId)
                          .FirstOrDefault(e => !string.IsNullOrEmpty(e) && e == fileName);
        Cache_[key] = result is not null;

        return Cache_[key];
    }
    public static string? GetAppUserModelId(this Package entry)
    {
        var key = entry.Id.FullName;

        if (Cache.TryGetValue(key, out var value)) return value;

        var result = entry.GetAppListEntries()
                          .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                          .Select(e => e.AppUserModelId)
                          .FirstOrDefault(e => !string.IsNullOrEmpty(e));
        Cache[key] = result ?? string.Empty;

        return Cache[key];
    }

    #endregion Methods
}