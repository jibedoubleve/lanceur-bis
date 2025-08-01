using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IThumbnailService
{
    #region Methods

    /// <summary>
    /// Asynchronously refresh the thumbnails. The methods lauches the threads
    /// and returns. Callbacks will set the property <see cref="QueryResult.Thumbnail"/>
    /// when thread has done its work
    /// </summary>
    /// <param name="queries">The <see cref="QueryResult"/> to refresh</param>
    void UpdateThumbnails(IEnumerable<QueryResult> queries);

    /// <summary>
    ///     Updates the thumbnail for the provided query. This method handles different types of sources:
    ///     executables, Windows Store applications, and URLs. It attempts to retrieve and assign the appropriate
    ///     thumbnail or favicon based on the query information.
    /// </summary>
    /// <param name="queryResult">An object containing the necessary information to retrieve and update the thumbnail.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateThumbnailAsync(QueryResult queryResult);

    #endregion Methods
}