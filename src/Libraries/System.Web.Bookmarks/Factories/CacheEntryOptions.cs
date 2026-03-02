using Humanizer;
using Microsoft.Extensions.Caching.Memory;

namespace System.Web.Bookmarks.Factories;

public abstract class CacheEntryOptions
{
    #region Properties

    public static MemoryCacheEntryOptions Default => new() { AbsoluteExpirationRelativeToNow = 2.Minutes() };

    #endregion
}