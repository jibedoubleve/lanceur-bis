namespace Lanceur.Core.Configuration.Configurations;

/// <summary>
///     Represents the configuration settings for the application with no cache as default..
///     These settings are stored directly within the database and define key behaviours and preferences.
/// </summary>
public class NoCacheApplicationSettings : ApplicationSettings
{
    #region Constructors

    public NoCacheApplicationSettings()
    {
        Caching.StoreCacheDuration = 0;
        Caching.ThumbnailCacheDuration = 0;
    }

    #endregion
}