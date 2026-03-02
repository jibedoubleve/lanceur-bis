using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.WPF.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Lanceur.Ui.WPF.Services;

public class ToastUserNotificationService : IUserGlobalNotificationService
{
    #region Fields

    private readonly LazyLoadedSynchronisationContext _dispatcher;
    private readonly ILogger<ToastUserNotificationService> _logger;

    #endregion

    #region Constructors

    public ToastUserNotificationService(
        LazyLoadedSynchronisationContext dispatcher,
        ILogger<ToastUserNotificationService> logger
    )
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    #endregion

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
        var icon = GetIconUri(level);
        new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void AskRestart()
    {
        const string msg = "To apply the changes, a restart of the application is required.";
        var icon = GetIconUri(Level.Information);
        var btnRestart = new ToastButton().SetContent("Restart Lanceur")
                                          .AddArgument("Type", ToastNotificationArguments.ClickRestart);

        new ToastContentBuilder()
            .AddText("Settings Updated")
            .AddText(msg)
            .AddButton(btnRestart)
            .AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void Error(string message, Exception ex)
    {
        var icon = GetIconUri(Level.Error);
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
            .AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void Error(string message)
    {
        var icon = GetIconUri(Level.Error);

        new ToastContentBuilder()
            .AddText("Error")
            .AddText(message)
            .AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void Information(string message) => Show(Level.Information, message);

    public void InformationWithNavigation(string message, string url)
    {
        var icon = GetIconUri(Level.Information);

        var btnNavigate = new ToastButton().SetContent("View Details")
                                           .AddArgument("Type", ToastNotificationArguments.ClickNavigateIssue);

        new ToastContentBuilder()
            .AddText("Information")
            .AddText(message)
            .AddArgument("Url", url)
            .AddButton(btnNavigate)
            .AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void NotifyNewVersionAvailable(Version version)
    {
        var msg = $"A new version {version} is now available!";
        var icon = GetIconUri(Level.Information);
        var btnCheckWebsite = new ToastButton().SetContent("Check the website")
                                               .AddArgument("Type", ToastNotificationArguments.VisitWebsite);
        var btnSkipVersion = new ToastButton().SetContent("Skip this version")
                                              .AddArgument("Type", ToastNotificationArguments.SkipVersion)
                                              .AddArgument("Version", version.ToString());

        new ToastContentBuilder()
            .AddText("New version available")
            .AddText(msg)
            .AddButton(btnCheckWebsite)
            .AddButton(btnSkipVersion)
            .AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    /// <inheritdoc />
    public void StartBusyIndicator()
    {
        _logger.LogTrace("Starting Busy Indicator");
        _dispatcher.Current.Post(_ => Mouse.OverrideCursor = Cursors.AppStarting, null);
    }

    /// <inheritdoc />
    public void StopBusyIndicator()
    {
        _logger.LogTrace("Stopping Busy Indicator");
        _dispatcher.Current.Post(_ => Mouse.OverrideCursor = null, null);
    }

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