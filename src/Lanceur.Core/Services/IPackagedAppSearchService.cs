using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IPackagedAppSearchService
{
    #region Methods

    /// <summary>
    ///     Retrieves a collection of packaged applications that match the specified
    ///     installed directory or application user model ID.
    /// </summary>
    /// <param name="fileName">
    ///     The file name to search for, with a "package:" prefix that will be removed
    ///     during processing. Used to determine the installed directory and app user model ID.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains
    ///     an enumerable collection of packaged applications matching the specified criteria.
    /// </returns>
    Task<IEnumerable<PackagedApp>> GetByInstalledDirectoryAsync(string fileName);

    /// <summary>
    ///     Retrieves all packaged applications currently installed on the system.
    /// </summary>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result contains
    ///     an enumerable collection of all installed packaged applications.
    /// </returns>
    Task<IEnumerable<PackagedApp>> GetInstalledUwpAppsAsync();

    /// <summary>
    ///     Attempts to populate the specified <see cref="AliasQueryResult" /> with details
    ///     of a packaged application that matches the provided file name.
    /// </summary>
    /// <param name="queryResult">
    ///     The query result object to populate with application details if a match is found.
    ///     Must not be null.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result is <c>true</c> if
    ///     the query result was successfully populated with packaged application details;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="queryResult" /> is null.
    /// </exception>
    Task<bool> TryResolveDetailsAsync(AliasQueryResult queryResult);

    #endregion
}