namespace Lanceur.Core.Configuration;

/// <summary>
///     Represents the caching configuration settings for the application.
/// </summary>
public class CachingSection
{
    public CachingSection(int storeCacheDuration, int thumbnailCacheDuration)
    {
        StoreCacheDuration = storeCacheDuration;
        ThumbnailCacheDuration = thumbnailCacheDuration;
    }
    #region Properties

    /// <summary>
    ///     Gets or sets the cache duration for stored data, in minutes.
    /// </summary>
    public int StoreCacheDuration { get; set; }

    /// <summary>
    ///     Gets or sets the cache duration for thumbnails, in minutes.
    /// </summary>
    public int ThumbnailCacheDuration { get; set; }

    #endregion
}