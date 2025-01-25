using System.Web.Bookmarks.Domain;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace System.Web.Bookmarks.Repositories;

/// <inheritdoc />
public class FireFoxBookmarkRepository(IMemoryCache memoryCache, ILoggerFactory loggerFactory) : SqlBookmarkRepository(
    memoryCache,
    loggerFactory,
    BrowserConfig.Firefox,
    "FireFox"
);