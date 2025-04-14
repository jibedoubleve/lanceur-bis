namespace Lanceur.Core.Services;

public interface IReleaseService
{
    #region Methods

    /// <summary>
    ///     Checks if a new release is available on GitHub.
    /// </summary>
    /// <returns>
    ///     A tuple indicating whether an update is available (<c>true</c>)
    ///     and the version of the latest release, if applicable.
    /// </returns>
    Task<(bool HasUpdate, Version Version)> HasUpdateAsync();

    #endregion
}