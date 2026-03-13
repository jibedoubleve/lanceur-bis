namespace Lanceur.Core.Services;

public interface IReleaseService
{
    #region Methods

    /// <summary>
    ///     Checks whether a new release is available on GitHub and notifies the user if appropriate.
    /// </summary>
    /// <remarks>
    ///     No notification is sent if no update is found or if the version check is snoozed.
    /// </remarks>
    Task CheckAndNotifyAsync();

    #endregion
}