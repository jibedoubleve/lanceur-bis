using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.WPF.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Lanceur.Ui.WPF.Services;

public class ToastUserNotificationService : IUserGlobalNotificationService
{
    #region Fields

    private readonly LazyLoadedSynchronizationContext _dispatcher;
    private readonly ILogger<ToastUserNotificationService> _logger;

    #endregion

    #region Constructors

    private static AppNotificationManager Notifier { get; } = AppNotificationManager.Default;

    static ToastUserNotificationService()
    {
        //Notifier.SetAppId("Probel.Lanceur");
        Notifier.Register(); // must be called once per process before Show
    }

    public ToastUserNotificationService(LazyLoadedSynchronizationContext dispatcher, ILogger<ToastUserNotificationService> logger)
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
        var notification = new AppNotificationBuilder()
            .AddText(title)
            .AddText(message)
            .SetHeroImage(icon.ToUriRelative())
            .BuildNotification();
        Notifier.Show(notification);
        
    }

    /// <inheritdoc />
    public void AskRestart()
    {
        const string message = "To apply the changes, a restart of the application is required.";
        var icon = GetIconUri(Level.Information);
        var btnRestart = new AppNotificationButton("Restart Lanceur")
            .AddArgument("Type", ToastNotificationArguments.ClickRestart);

        new AppNotificationBuilder()
            .AddText("Settings Updated")
            .AddText(message)
            .AddButton(btnRestart)
            .SetHeroImage(icon.ToUriRelative())
            .BuildNotification();
    }

    /// <inheritdoc />
    public void Error(string message, Exception ex)
    {
        var icon = GetIconUri(Level.Error);
        var btnError = new AppNotificationButton("Show Error")
                       .AddArgument("Type", ToastNotificationArguments.ClickShowError)
                       .AddArgument("Message", message)
                       .AddArgument("StackTrace", ex.ToString());

        var btnLogs = new AppNotificationButton("Show Logs")
            .AddArgument("Type", ToastNotificationArguments.ClickShowLogs);

        new AppNotificationBuilder()
            .AddText("Error")
            .AddText(message)
            .AddButton(btnError)
            .AddButton(btnLogs)
            .SetHeroImage(icon.ToUriRelative())
            .BuildNotification();
    }

    /// <inheritdoc />
    public void Error(string message)
    {
        var icon = GetIconUri(Level.Error);

        new AppNotificationBuilder()
            .AddText("Error")
            .AddText(message)
            .SetHeroImage(icon.ToUriRelative())
            .BuildNotification();
    }

    /// <inheritdoc />
    public void Information(string message) => Show(Level.Information, message);

    public void InformationWithNavigation(string message, string url)
    {
        var icon = GetIconUri(Level.Information);

        var btnNavigate = new AppNotificationButton("View Details")
            .AddArgument("Type", ToastNotificationArguments.ClickNavigateIssue);

        new AppNotificationBuilder()
            .AddText("Information")
            .AddText(message)
            .AddArgument("Url", url)
            .AddButton(btnNavigate)
            //.AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .BuildNotification();
    }

    /// <inheritdoc />
    public void NotifyNewVersionAvailable(Version version)
    {
        var msg = $"A new version {version} is now available!";
        var icon = GetIconUri(Level.Information);
        var btnCheckWebsite = new AppNotificationButton("Check the website")
            .AddArgument("Type", ToastNotificationArguments.VisitWebsite);
        var btnSkipVersion = new AppNotificationButton("Skip this version")
                             .AddArgument("Type", ToastNotificationArguments.SkipVersion)
                             .AddArgument("Version", version.ToString());

        new AppNotificationBuilder()
            .AddText("New version available")
            .AddText(msg)
            .AddButton(btnCheckWebsite)
            .AddButton(btnSkipVersion)
            //.AddAppLogoOverride(icon.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .BuildNotification();
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