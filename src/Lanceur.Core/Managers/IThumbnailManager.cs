using Lanceur.Core.Models;

namespace Lanceur.Core.Managers;

public interface IThumbnailManager
{
    #region Methods

    /// <summary>
    /// Asynchronously refresh the thumbnails. The methods lauches the threads
    /// and returns. Callbacks will set the property <see cref="QueryResult.Thumbnail"/>
    /// when thread has done its work
    /// </summary>
    /// <param name="results">The <see cref="QueryResult"/> to refresh</param>
    void RefreshThumbnailsAsync(IEnumerable<QueryResult> results);

    #endregion Methods
}