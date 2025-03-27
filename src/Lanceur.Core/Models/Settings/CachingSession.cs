namespace Lanceur.Core.Models.Settings;

/// <summary>
///     Represents the caching configuration settings for the application.
/// </summary>
public class CachingSession
{
    #region Properties

    /// <summary>
    ///     Gets or sets the cache duration for stored data, in minutes.
    /// </summary>
    public int StoreCacheDuration { get; set; } = 30;

    /// <summary>
    ///     Gets or sets the cache duration for thumbnails, in minutes.
    /// </summary>
    public int ThumbnailCacheDuration { get; set; } = 30;

    #endregion
}