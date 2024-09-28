using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.Services;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Lanceur.Ui.WPF.Services;

public class ToastUiNotificationService : IUiNotificationService
{    
    #region Enums

    private enum Level { Information, Warning, Error };

    #endregion Enums

    #region Methods

    private static void Show(Level level, string message, [CallerMemberName] string? title = null)
    {
        var uri = level switch
        {
            Level.Information => Icon.Info,
            Level.Warning     => Icon.Warn,
            Level.Error       => Icon.Error,
            _                 => Icon.None
        };
        new ToastContentBuilder()
            .AddText(title)
            .AddText(message)
            .AddAppLogoOverride(uri.ToUriRelative(), ToastGenericAppLogoCrop.Circle)
            .Show();
    }

    public void Error(string message) => Show(Level.Error, message);

    public void Information(string message) => Show(Level.Information, message);

    public void Warning(string message) => Show(Level.Warning, message);

    #endregion Methods

    #region Classes

    private static class Icon
    {
        #region Constructors

        static Icon()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            Info = Path.Combine(path, @"Assets\IconInfo.png");
            Warn = Path.Combine(path, @"Assets\IconWarn.png");
            Error = Path.Combine(path, @"Assets\IconError.png");
        }

        #endregion Constructors

        #region Properties

        public static string Error { get; }
        public static string Info { get; }
        public static string None => string.Empty;
        public static string Warn { get; }

        #endregion Properties
    };

    #endregion Classes
}