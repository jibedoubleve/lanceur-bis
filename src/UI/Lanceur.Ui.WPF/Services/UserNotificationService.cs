using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Messages;

namespace Lanceur.Ui.WPF.Services;

public class UserNotificationService : IUserNotificationService
{
    #region Methods

    private static void Send(MessageLevel level, string title, string message)
        => WeakReferenceMessenger.Default.Send(new NotificationMessage((level, title, message)));

    /// <inheritdoc />
    public void DisableLoadingState() => Mouse.OverrideCursor = null;

    /// <inheritdoc />
    public void EnableLoadingState() => Mouse.OverrideCursor = Cursors.Wait;

    /// <inheritdoc />
    public void Success(string message, string title = "Success") => Send(MessageLevel.Success, title, message);

    /// <inheritdoc />
    public IDisposable TrackLoadingState() => new WaitScope(this);

    /// <inheritdoc />
    public void Warning(string message, string title = "Warning") => Send(MessageLevel.Warning, title, message);

    #endregion

    private class WaitScope : IDisposable
    {
        #region Fields

        private readonly IUserNotificationService _userInteractionService;

        #endregion

        #region Constructors

        internal WaitScope(IUserNotificationService userInteractionService)
        {
            _userInteractionService = userInteractionService;
            _userInteractionService.EnableLoadingState();
        }

        #endregion

        #region Methods

        public void Dispose() { _userInteractionService.DisableLoadingState(); }

        #endregion
    }
}