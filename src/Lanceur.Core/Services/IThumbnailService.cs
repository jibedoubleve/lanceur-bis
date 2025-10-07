using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IThumbnailService
{
    #region Methods

    /// <summary>
    /// Asynchronously refresh the thumbnails. The methods launches the thread
    /// and returns. Callbacks will set the property <see cref="QueryResult.Thumbnail"/>
    /// when thread has done its work
    /// </summary>
    /// <param name="queryResult">The <see cref="QueryResult"/> to refresh</param>
    void UpdateThumbnail(QueryResult queryResult);

    #endregion Methods
}