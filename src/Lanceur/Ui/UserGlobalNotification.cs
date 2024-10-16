using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
using Splat;
using System;
using System.Net.Mime;
using System.Windows;
using Lanceur.Utils;

namespace Lanceur.Ui;

public class UiNotification : IUiNotification
{
    #region Fields

    private readonly ILogger<UiNotification> _logger;
    private readonly INotification _notification;

    #endregion Fields

    #region Constructors

    public UiNotification(ILoggerFactory logFactory = null, INotification notification = null)
    {
        _logger = logFactory.GetLogger<UiNotification>();
        _notification = notification ?? Locator.Current.GetService<INotification>();
        ;
    }

    #endregion Constructors

    #region Methods

    private static void HandleCrashingNotification(string message, Exception ex, Exception e)
    {
        Locator.Current.GetLogger<UiNotification>()
               .LogWarning(ex, "User notification failed ({Message}). Show message in a MessageBox", e.Message);
        MessageBox.Show(Application.Current.MainWindow!, $"{message}. {(ex is null ? "" : $"{Environment.NewLine}{ex}")}", "Warning");
    }

    //TODO: refactor logging... It's not optimised
    public void Error(string message, Exception ex)
    {
        try
        {
            _logger.LogError(ex, "An error occured. {Message}", ex.Message);
            _notification.Error(message);
        }
        catch (Exception e) { HandleCrashingNotification(message, ex, e); }
    }

    //TODO: refactor logging... It's not optimised
    public void Warning(string message, Exception ex = null)
    {
        try
        {
            _logger.LogWarning(ex, "{Message}", message);
            _notification.Warning(message);
        }
        catch (Exception e) { HandleCrashingNotification(message, ex, e); }
    }

    #endregion Methods
}