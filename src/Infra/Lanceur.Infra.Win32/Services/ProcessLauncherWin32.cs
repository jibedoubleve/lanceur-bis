using System.Diagnostics;
using System.IO;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Mappers;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public sealed class ProcessLauncherWin32 : IProcessLauncher
{
    #region Fields
    private readonly ILogger<ProcessLauncherWin32> _logger;
    private readonly SearchBoxSection _settings;

    private readonly IUserGlobalNotificationService _notificationService;

    #endregion

    #region Constructors

    public ProcessLauncherWin32(
        IUserGlobalNotificationService notificationService,
        ILogger<ProcessLauncherWin32> logger,
        ISection<SearchBoxSection> settings)
    {
        _notificationService = notificationService;
        _logger = logger;
        _settings = settings.Value;
    }

    #endregion

    #region Methods

    internal void TrackStartup(ITrackedProcess process, string fileName)
    {
        using (process)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var isReady = process.WaitForInputIdle(_settings.SlowStartupMs);
                if (!isReady)
                {
                    using var _ =
                        _notificationService.Progress("Starting", $"Starting {fileName} ...");
                    isReady = process.WaitForInputIdle(_settings.StartupTimeout - _settings.SlowStartupMs);
                }

                if (isReady)
                {
                    _logger.LogInformation("{FileName}: ready after {Elapsed} ms", fileName, watch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("{FileName}: not ready after {Elapsed} ms", fileName, watch.ElapsedMilliseconds);
                    _notificationService.Warning(
                        $"Process '{fileName}' is not ready after {watch.Elapsed.Seconds} seconds"
                    );
                }
            }
            catch (InvalidOperationException ex) when (process.HasExited)
            {
                var logLevel = process.ExitCode == 0 ? LogLevel.Information : LogLevel.Warning;
                _logger.Log(
                    logLevel,
                    ex,
                    "{FileName}: exited after {Elapsed} ms with code {ExitCode}",
                    fileName,
                    watch.ElapsedMilliseconds,
                    process.ExitCode
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogInformation(ex, "{FileName}: no GUI to wait for", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{FileName}: cannot track startup", fileName);
            }
        }
    }

    /// <inheritdoc />
    public void Open(string path)
        => Process.Start(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"),
            path
        );

    /// <inheritdoc />
    public void Start(ProcessContext context)
    {
        var process = new Process { StartInfo = context.ToProcessStartInfo() };
        bool isStarted;
        try { isStarted = process.Start(); }
        catch (Exception)
        {
            process.Dispose();
            throw;
        }

        if (!isStarted)
        {
            _logger.LogInformation("{FileName}: handled by the shell, no process to track", context.FileName);
            process.Dispose();
            return;
        }

        _logger.LogInformation("{FileName}: started with PID {ProcessId}", context.FileName, process.Id);
        _ = Task.Run(() => TrackStartup(new TrackedProcess(process), context.FileName));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> Start(ISelfExecutable executable, Cmdline cmdline)
        => await executable.ExecuteAsync(cmdline);

    #endregion
}