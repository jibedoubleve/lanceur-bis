namespace Lanceur.Core.Services;

public interface IMessageService
{
    #region Methods

    /// <summary>
    ///     Sends the specified message using the configured message service.
    /// </summary>
    /// <param name="message">An object representing the message to send. Must not be null.</param>
    void Send(object message);

    #endregion
}