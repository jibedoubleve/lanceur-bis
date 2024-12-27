using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.Messages;

namespace Lanceur.Ui.WPF.Services;

public class UserNotificationService : IUserNotificationService
{
    private class WaitScope : IDisposable
    {
        private readonly IUserNotificationService _userInteractionService;

        internal WaitScope(IUserNotificationService userInteractionService)
        {
            _userInteractionService = userInteractionService;
            _userInteractionService.EnableLoadingState();
        }

        public void Dispose() { _userInteractionService.DisableLoadingState(); }
    }

    #region Methods

    private void Send(MessageLevel level, string title, string message) => WeakReferenceMessenger.Default.Send(new NotificationMessage((level, title, message)));

    /// <inheritdoc />
    public void EnableLoadingState() => Mouse.OverrideCursor = Cursors.Wait;

    /// <inheritdoc />
    public void DisableLoadingState() => Mouse.OverrideCursor = null;

    /// <inheritdoc />
    public IDisposable TrackLoadingState() => new WaitScope(this);

    /// <inheritdoc />
    public void Success(string message, string title) => Send(MessageLevel.Success, title, message);

    /// <inheritdoc />
    public void Warn(string message, string title) => Send(MessageLevel.Warning, title, message);

    #endregion
}