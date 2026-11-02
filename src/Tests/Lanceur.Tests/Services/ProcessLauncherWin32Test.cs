using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class ProcessLauncherWin32Test
{
    #region Fields

    private const string FileName = "app.exe";
    private const int SlowStartupMs = 100;
    private const int StartupTimeout = 1_000;

    private readonly IUserGlobalNotificationService _notifications = Substitute.For<IUserGlobalNotificationService>();
    private readonly ITestOutputHelper _output;
    private readonly IDisposable _progress = Substitute.For<IDisposable>();

    #endregion

    #region Constructors

    public ProcessLauncherWin32Test(ITestOutputHelper output)
    {
        _output = output;
        _notifications.Progress(Arg.Any<string>(), Arg.Any<string>()).Returns(_progress);
    }

    #endregion

    #region Methods

    private ProcessLauncherWin32 CreateSut()
    {
        var settings = Substitute.For<ISection<SearchBoxSection>>();
        settings.Value.Returns(new SearchBoxSection { SlowStartupMs = SlowStartupMs, StartupTimeout = StartupTimeout });

        return new ProcessLauncherWin32(
            _notifications,
            new TestOutputHelperDecoratorForMicrosoftLogging<ProcessLauncherWin32>(_output),
            settings
        );
    }

    [Fact]
    public void When_process_has_exited_Then_no_warning_is_shown()
    {
        var process = new FakeProcess { HasExited = true };

        CreateSut().TrackStartup(process, FileName);

        _notifications.DidNotReceive().Warning(Arg.Any<string>());
    }

    [Fact]
    public void When_process_is_ready_before_slow_threshold_Then_no_notification_is_shown()
    {
        var process = new FakeProcess(true);

        CreateSut().TrackStartup(process, FileName);

        Assert.Multiple(
            () => _notifications.DidNotReceive().Progress(Arg.Any<string>(), Arg.Any<string>()),
            () => _notifications.DidNotReceive().Warning(Arg.Any<string>())
        );
    }

    [Fact]
    public void When_process_never_becomes_ready_Then_warning_is_shown_and_progress_is_disposed()
    {
        var process = new FakeProcess(false, false);

        CreateSut().TrackStartup(process, FileName);

        Assert.Multiple(
            () => _notifications.Received(1).Warning(Arg.Any<string>()),
            () => _progress.Received(1).Dispose()
        );
    }

    [Fact]
    public void When_startup_is_slow_Then_progress_is_shown_and_disposed()
    {
        var process = new FakeProcess(false, true);

        CreateSut().TrackStartup(process, FileName);

        Assert.Multiple(
            () => _notifications.Received(1).Progress(Arg.Any<string>(), Arg.Any<string>()),
            () => _progress.Received(1).Dispose(),
            () => _notifications.DidNotReceive().Warning(Arg.Any<string>())
        );
    }

    [Fact]
    public void When_tracking_startup_Then_waits_use_slow_threshold_then_remaining_time()
    {
        var process = new FakeProcess(false, false);

        CreateSut().TrackStartup(process, FileName);

        process.Waits.ShouldBe([SlowStartupMs, StartupTimeout - SlowStartupMs]);
    }

    #endregion

    #region Classes

    /// <summary>
    ///     Replays the given readiness results, one per wait, and records the delay of each wait.
    ///     When the process has exited, a wait throws as <see cref="System.Diagnostics.Process" /> does.
    /// </summary>
    private sealed class FakeProcess(params bool[] readiness) : ITrackedProcess
    {
        #region Fields

        private readonly Queue<bool> _readiness = new(readiness);

        #endregion

        #region Properties

        public int ExitCode => 0;

        public bool HasExited { get; init; }

        public List<int> Waits { get; } = [];

        #endregion

        #region Methods

        public void Dispose() { }

        public bool WaitForInputIdle(int milliseconds)
        {
            if (HasExited) { throw new InvalidOperationException("Process has exited."); }

            Waits.Add(milliseconds);
            return _readiness.Dequeue();
        }

        #endregion
    }

    #endregion
}