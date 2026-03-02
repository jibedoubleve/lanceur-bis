using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public class AppRestartDummyService : IAppRestartService
{
    #region Fields

    private readonly ILogger<AppRestartDummyService> _notification;

    #endregion

    #region Constructors

    public AppRestartDummyService(ILogger<AppRestartDummyService> notification) => _notification = notification;

    #endregion

    #region Methods

    /// <inheritdoc />
    public void HoldInstance() { }

    /// <inheritdoc />
    public void ReleaseInstance() { }

    /// <inheritdoc />
    public void Restart()
    {
        _notification.LogInformation(
            "Application restart requested. Note: This is a placeholder restarter for development purposes only."
        );
    }

    #endregion
}