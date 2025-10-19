using System.Diagnostics;
using System.Runtime.CompilerServices;
using Lanceur.Core.Services;
using Lanceur.Infra.Mappers;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class ProcessLauncherWin32 : IProcessLauncher
{
    #region Fields

    private readonly IUserGlobalNotificationService _notificationService;
    private readonly ILogger<ProcessLauncherWin32> _logger;

    #endregion

    #region Constructors

    public ProcessLauncherWin32(IUserGlobalNotificationService notificationService, ILogger<ProcessLauncherWin32> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Open(string path) => Process.Start("explorer.exe", path);

    /// <inheritdoc />
    public void Start(ProcessContext context)
    {
        var process = new Process { StartInfo = context.ToProcessStartInfo(), EnableRaisingEvents = true };
        _notificationService.StartBusyIndicator();

        if (!process.Start())
        {
            _logger.LogInformation("Process {ProcessName} failed to start.", context.FileName);
        }

        _ = Task.Run(() =>
            {
                try { process.WaitForInputIdle(5_000); }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Process {ProcessName} failed to start.", context.FileName);
                }
                finally { _notificationService.StopBusyIndicator(); }
            }
        );
    }

    #endregion
}