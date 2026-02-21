using System.Diagnostics;
using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Mappers;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public class ProcessLauncherWin32 : IProcessLauncher
{
    #region Fields

    private readonly ILogger<ProcessLauncherWin32> _logger;

    private readonly IUserGlobalNotificationService _notificationService;

    #endregion

    #region Constructors

    public ProcessLauncherWin32(
        IUserGlobalNotificationService notificationService,
        ILogger<ProcessLauncherWin32> logger
    )
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

        if (!process.Start()) _logger.LogInformation("Process {ProcessName} failed to start.", context.FileName);

        _ = Task.Run(() =>
            {
                try
                {
                    _notificationService.StartBusyIndicator();
                    process.WaitForInputIdle(5_000);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Process {ProcessName} failed to start.", context.FileName);
                }
                finally { _notificationService.StopBusyIndicator(); }
            }
        );
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> Start(ISelfExecutable executable, Cmdline cmdline)
        => await  executable.ExecuteAsync(cmdline);

    #endregion
}