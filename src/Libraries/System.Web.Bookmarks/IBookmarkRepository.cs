﻿using System.Web.Bookmarks.Domain;

namespace System.Web.Bookmarks;

public interface IBookmarkRepository
{
    #region Properties

    /// <summary>
    ///     Represent the name of the cache where the bookmarks are saved
    /// </summary>
    public string CacheKey { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Retrieves all the bookmarks.
    /// </summary>
    /// <returns>
    ///     A collection of all <see cref="Bookmark" /> objects.
    /// </returns>
    IEnumerable<Bookmark> GetBookmarks();

    /// <summary>
    ///     Retrieves all bookmarks that contain the specified query string in their name.
    /// </summary>
    /// <param name="filter">
    ///     The substring to search for within the names of the bookmarks. Case sensitivity depends on the implementation.
    /// </param>
    /// <returns>
    ///     A collection of <see cref="Bookmark" /> objects whose names contain the specified query string.
    /// </returns>
    IEnumerable<Bookmark> GetBookmarks(string filter);

    /// <summary>
    ///     Determines whether the specified browser is installed on the system
    ///     and whether its bookmark configuration is accessible.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if the browser is installed and its bookmark configuration can be found;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     A return value of <c>false</c> indicates that either the browser is not installed,
    ///     or its bookmark configuration is missing or inaccessible.
    /// </remarks>
    bool IsBookmarkSourceAvailable();

    /// <summary>
    ///     Clears the cached bookmarks.
    /// </summary>
    /// <remarks>
    ///     This method should be called when the bookmark data has changed externally,
    ///     and the cache needs to be invalidated to ensure fresh data is retrieved.
    /// </remarks>
    void InvalidateCache();


    #endregion
}