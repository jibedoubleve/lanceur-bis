using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace System.Web.Bookmarks.Repositories;

public class EdgeBookmarksRepository(IMemoryCache memoryCache) : ChromiumBookmarksRepository(memoryCache)
{
    #region Properties

    protected override string CacheKey => $"ChromeBookmarks_{nameof(ChromiumBookmarksRepository)}";

    protected override string Path => @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Bookmarks".ExpandPath();

    #endregion
}