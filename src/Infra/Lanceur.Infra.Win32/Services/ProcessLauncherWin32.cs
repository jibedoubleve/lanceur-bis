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

internal sealed class ProcessLauncherWin32 : IProcessLauncher
{
    #region Fields

    private readonly ILogger<ProcessLauncherWin32> _logger;

    private readonly IUserGlobalNotificationService _notificationService;
    private readonly IProcessLookup _processLookup;
    private readonly ProcessLaunchSection _settings;

    #endregion

    #region Constructors

    public ProcessLauncherWin32(
        IUserGlobalNotificationService notificationService,
        ILogger<ProcessLauncherWin32> logger,
        ISection<ProcessLaunchSection> settings,
        IProcessLookup processLookup)
    {
        _notificationService = notificationService;
        _logger = logger;
        _processLookup = processLookup;
        _settings = settings.Value;
    }

    #endregion

    #region Methods

    internal async Task TrackStartupAsync(ITrackedProcess process, string fileName, ProcessSnapshot snapshot)
    {
        using (process)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var isReady = process.WaitForInputIdle(_settings.SlowStartupThreshold);
                if (!isReady) // Notify slow startup
                {
                    using var _ =
                        _notificationService.Progress("Starting", $"Starting {fileName} ...");
                    isReady = process.WaitForInputIdle(_settings.StartupTimeout - _settings.SlowStartupThreshold);
                }

                if (isReady) // Process is ready and running
                {
                    _logger.LogInformation("{FileName}: ready after {Elapsed} ms", fileName, watch.ElapsedMilliseconds);
                    await TryHandleEarlyExit();
                }
                else // Timeout: if app isn't started yet, this is not expected; inform user.
                {
                    _logger.LogWarning("{FileName}: not ready after {Elapsed} ms", fileName, watch.ElapsedMilliseconds);
                    _notificationService.Warning(
                        $"Process '{fileName}' is not ready after {watch.Elapsed.Seconds} seconds"
                    );
                }
            }
            catch (InvalidOperationException ex) when (process.HasExited)
            {
                if (await TryHandleEarlyExit()) { return; }

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

            //----------
            async Task<bool> TryHandleEarlyExit()
            {
                try
                {
                    if (snapshot.IsEmpty) { return false; }

                    // Wait some time and check the snapshot
                    await Task.Delay(_settings.EarlyExitCheckDelay);

                    snapshot.Add(process.Id);
                    if (snapshot.AnyAlive()) { return false; }

                    _logger.LogWarning(
                        "{FileName}: process {ProcessId} closed within {Delay} ms of starting and no other instance is running",
                        fileName,
                        process.Id,
                        _settings.EarlyExitCheckDelay
                    );
                    _notificationService.Warning(
                        $"'{Path.GetFileName(fileName)}' closed right after starting and no other instance is running. Try running the alias again."
                    );
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "{FileName}: cannot check whether the process closed right after starting",
                        fileName);
                    return false;
                }
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
        var snapshot = ProcessSnapshot.Capture(_processLookup, Path.GetFileNameWithoutExtension(context.FileName));
        bool isStarted;

        try
        {
            isStarted = process.Start();
        }
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
        _ = Task.Run(() => TrackStartupAsync(
            new TrackedProcess(process),
            context.FileName,
            snapshot
        ));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> Start(ISelfExecutable executable, Cmdline cmdline)
        => await executable.ExecuteAsync(cmdline);

    #endregion
}