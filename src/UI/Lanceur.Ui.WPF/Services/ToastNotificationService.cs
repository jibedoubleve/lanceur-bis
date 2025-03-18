using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Lanceur.Ui.WPF.Services;

public class ToastUserNotificationService : IUserGlobalNotificationService
{
    #region Methods

    private static string GetIconUri(Level level)
    {
        var uri = level switch
        {
            Level.Information => Icon.Info,
            Level.Warning     => Icon.Warn,
            Level.Error       => Icon.Error,
            _                 => Icon.None
        };
        return uri;
    }

    private static void Show(Level level, string message, [CallerMemberName] string? title = null)
    {
        var uri = GetIconUri(level);
        new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .AddAppLogoOverride(uri.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void AskRestart()
    {
        const string msg = "To apply the changes, a restart of the application is required.  \n\nPlease click \"Restart Application\" to continue.";
        var uri = GetIconUri(Level.Information);
        var btnRestart = new ToastButton().SetContent("Restart Lanceur")
                                          .AddArgument("Type", ToastNotificationArguments.ClickRestart);

        new ToastContentBuilder()
            .AddText("Settings Updated")
            .AddText(msg)
            .AddButton(btnRestart)
            .AddAppLogoOverride(uri.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void Error(string message, Exception ex)
    {
        var uri = GetIconUri(Level.Error);
        var btnError = new ToastButton().SetContent("Show Error")
                                        .AddArgument("Type", ToastNotificationArguments.ClickShowError)
                                        .AddArgument("Message", message)
                                        .AddArgument("StackTrace", ex.ToString());

        var btnLogs = new ToastButton().SetContent("Show Logs")
                                       .AddArgument("Type", ToastNotificationArguments.ClickShowLogs);

        new ToastContentBuilder()
            .AddText("Error")
            .AddText(message)
            .AddButton(btnError)
            .AddButton(btnLogs)
            .AddAppLogoOverride(uri.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void Information(string message) => Show(Level.Information, message);

    /// <inheritdoc />
    public void Warning(string message) => Show(Level.Warning, message);

    #endregion

    #region Enums

    private enum Level { Information, Warning, Error }

    #endregion Enums

    #region Classes

    private static class Icon
    {
        #region Constructors

        static Icon()
        {
            var path = Assembly.GetExecutingAssembly().Location.GetDirectoryName()!;
            Info = Path.Combine(path, @"Assets\IconInfo.png");
            Warn = Path.Combine(path, @"Assets\IconWarn.png");
            Error = Path.Combine(path, @"Assets\IconError.png");
        }

        #endregion

        #region Properties

        public static string Error { get; }
        public static string Info { get; }
        public static string None => string.Empty;
        public static string Warn { get; }

        #endregion
    }

    #endregion Classes
}