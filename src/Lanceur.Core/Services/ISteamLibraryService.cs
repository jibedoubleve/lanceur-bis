using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
///     Provides access to the Steam game library installed on the user's machine.
/// </summary>
public interface ISteamLibraryService
{
    #region Methods

    /// <summary>
    ///     Retrieves all Steam games installed on the user's machine.
    /// </summary>
    /// <returns>A collection of installed Steam games.</returns>
    IEnumerable<SteamGame> GetGames();

    #endregion
}