using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Restart;

public class DummyAppRestart : IAppRestart
{
    private readonly ILogger<DummyAppRestart> _notification;
    public DummyAppRestart(ILogger<DummyAppRestart> notification) { _notification = notification;  }

    #region Methods

    public void Restart()
    {
        _notification.LogInformation("Application restart requested. Note: This is a placeholder restarter for development purposes only.");
    }


    #endregion Methods
}