using Lanceur.Core.Models;

namespace Lanceur.Infra.Win32.Thumbnails;

/// <summary>
///     Defines a strategy for updating the thumbnail of an alias.
///     Each implementation handles a specific type of application
///     (Win32, packaged app, web app favicon, etc.).
/// </summary>
public interface IThumbnailStrategy
{
    #region Properties

    /// <summary>
    ///     Determines the execution order of this strategy in the pipeline.
    ///     A lower value means the strategy is executed earlier.
    /// </summary>
    int Priority { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Updates the thumbnail associated with the specified alias.
    /// </summary>
    /// <param name="alias">The alias whose thumbnail needs to be updated.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    ///     <c>true</c> if the thumbnail was resolved (already cached or successfully fetched),
    ///     allowing the caller to skip remaining strategies; <c>false</c> if this strategy
    ///     could not resolve the thumbnail and the next strategy should be tried.
    /// </returns>
    Task<bool> UpdateThumbnailAsync(AliasQueryResult alias, CancellationToken cancellationToken);

    #endregion
}