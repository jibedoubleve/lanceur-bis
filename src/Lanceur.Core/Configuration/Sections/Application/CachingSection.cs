namespace Lanceur.Core.Configuration.Sections.Application;

/// <summary>
///     Represents the caching configuration settings for the application.
/// </summary>
public sealed class CachingSection
{
    #region Constructors

    public CachingSection(int storeCacheDuration, int thumbnailCacheDuration)
    {
        StoreCacheDuration = storeCacheDuration;
        ThumbnailCacheDuration = thumbnailCacheDuration;
    }

    #endregion

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