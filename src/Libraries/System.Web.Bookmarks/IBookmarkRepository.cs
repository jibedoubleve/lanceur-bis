namespace System.Web.Bookmarks;

public interface IBookmarkRepository
{
    #region Methods

    /// <summary>
    /// Retrieves all the bookmarks.
    /// </summary>
    /// <returns>
    /// A collection of all <see cref="Bookmark"/> objects.
    /// </returns>
    IEnumerable<Bookmark> GetBookmarks();

    /// <summary>
    /// Retrieves all bookmarks that contain the specified query string in their name.
    /// </summary>
    /// <param name="filter">
    /// The substring to search for within the names of the bookmarks. Case sensitivity depends on the implementation.
    /// </param>
    /// <returns>
    /// A collection of <see cref="Bookmark"/> objects whose names contain the specified query string.
    /// </returns>
    IEnumerable<Bookmark> GetBookmarks(string filter);
    
    #endregion
}