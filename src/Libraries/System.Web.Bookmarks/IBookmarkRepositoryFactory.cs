namespace System.Web.Bookmarks;

public interface IBookmarkRepositoryFactory
{
    #region Methods

    /// <summary>
    ///     Creates or retrieves a bookmark repository specific to the given browser.
    /// </summary>
    /// <param name="browser">The browser for which the bookmark repository is required (e.g., Chrome, Firefox, Zen).</param>
    /// <returns>
    ///     An instance of <see cref="IBookmarkRepository" /> configured for the specified browser.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown if the specified browser is not found.
    /// </exception>
    IBookmarkRepository BuildBookmarkRepository(string browser);

    #endregion
}