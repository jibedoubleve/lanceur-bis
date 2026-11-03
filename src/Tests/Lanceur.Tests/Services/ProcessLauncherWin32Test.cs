using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Tests.Tools.Stubs;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class ProcessLauncherWin32Test
{
    #region Fields

    private const string FileName = "app.exe";
    private const int SlowStartupThreshold = 100;
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

    private ProcessLauncherWin32 CreateSut(
        int slowStartupMs = 100, int startupTimeout = 1_000, int earlyExitCheckDelay = 3_000)
    {
        var settings = Substitute.For<ISection<ProcessLaunchSection>>();
        settings.Value.Returns(
            new ProcessLaunchSection
            {
                SlowStartupThreshold = slowStartupMs,
                StartupTimeout = startupTimeout,
                EarlyExitCheckDelay = earlyExitCheckDelay
            }
        );

        return new ProcessLauncherWin32(
            _notifications,
            new TestOutputHelperDecoratorForMicrosoftLogging<ProcessLauncherWin32>(_output),
            settings,
            new FakeProcessLookup()
        );
    }

    [Theory]
    [InlineData(new[] { FakeProcess.OldPid }, false)] // Hand over to a healthy instance
    [InlineData(new[] { FakeProcess.NewPid }, false)] // Normal launch, new process runs
    [InlineData(new[] { FakeProcess.OldPid, FakeProcess.NewPid }, false)]
    [InlineData(new int[0], true)] // CONFLICT : All process are gone...
    public async Task When_an_instance_was_running_before_Then_warns_only_if_none_survives(
        int[] aliveAfter, bool expectWarning)
    {
        var lookup = new FakeProcessLookup(aliveAfter);
        var snapshot = new ProcessSnapshot(lookup, [FakeProcess.OldPid]);

        await CreateSut(0, 0, 0).TrackStartupAsync(
            new FakeProcess(true) { Id = FakeProcess.NewPid },
            FileName,
            snapshot
        );

        if (expectWarning)
        {
            _notifications.Received(1).Warning(Arg.Is<string>(m => m.Contains(FileName)));
        }
        else
        {
            _notifications.DidNotReceive().Warning(Arg.Any<string>());
        }
    }

    [Fact]
    public async Task When_no_instance_was_running_before_Then_no_warnings()
    {
        var lookup = new FakeProcessLookup();
        var snapshot = new ProcessSnapshot(lookup, []);

        await CreateSut(0, 0, 0).TrackStartupAsync(
            new FakeProcess(true) { Id = FakeProcess.NewPid },
            FileName,
            snapshot
        );

        Assert.Multiple(
            () => lookup.IsAliveCalls.ShouldBe(0),
            () => _notifications.DidNotReceive().Warning(Arg.Any<string>())
        );
    }

    [Fact]
    public async Task When_process_exits_before_ready_and_no_instance_survives_Then_warning_is_shown()
    {
        var lookup = new FakeProcessLookup(); // plus rien de vivant
        var snapshot = new ProcessSnapshot(lookup, [FakeProcess.OldPid]);

        await CreateSut(earlyExitCheckDelay: 0)
            .TrackStartupAsync(new FakeProcess { HasExited = true }, FileName, snapshot);

        _notifications.Received(1).Warning(Arg.Is<string>(m => m.Contains(FileName)));
    }

    [Fact]
    public async Task When_process_has_exited_Then_no_warning_is_shown()
    {
        var process = new FakeProcess { HasExited = true };

        await CreateSut().TrackStartupAsync(process, FileName, new ProcessSnapshot(new ProcessLookup(), []));

        _notifications.DidNotReceive().Warning(Arg.Any<string>());
    }

    [Fact]
    public async Task When_process_is_ready_before_slow_threshold_Then_no_notification_is_shown()
    {
        var process = new FakeProcess(true);

        await CreateSut().TrackStartupAsync(process, FileName, new ProcessSnapshot(new ProcessLookup(), []));

        Assert.Multiple(
            () => _notifications.DidNotReceive().Progress(Arg.Any<string>(), Arg.Any<string>()),
            () => _notifications.DidNotReceive().Warning(Arg.Any<string>())
        );
    }

    [Fact]
    public async Task When_process_never_becomes_ready_Then_warning_is_shown_and_progress_is_disposed()
    {
        var process = new FakeProcess(false, false);

        await CreateSut().TrackStartupAsync(process, FileName, new ProcessSnapshot(new ProcessLookup(), []));

        Assert.Multiple(
            () => _notifications.Received(1).Warning(Arg.Any<string>()),
            () => _progress.Received(1).Dispose()
        );
    }

    [Fact]
    public async Task When_startup_is_slow_Then_progress_is_shown_and_disposed()
    {
        var process = new FakeProcess(false, true);

        await CreateSut().TrackStartupAsync(process, FileName, new ProcessSnapshot(new ProcessLookup(), []));

        Assert.Multiple(
            () => _notifications.Received(1).Progress(Arg.Any<string>(), Arg.Any<string>()),
            () => _progress.Received(1).Dispose(),
            () => _notifications.DidNotReceive().Warning(Arg.Any<string>())
        );
    }

    [Fact]
    public async Task When_tracking_startup_Then_waits_use_slow_threshold_then_remaining_time()
    {
        var process = new FakeProcess(false, false);

        await CreateSut().TrackStartupAsync(process, FileName, new ProcessSnapshot(new ProcessLookup(), []));

        process.Waits.ShouldBe([SlowStartupThreshold, StartupTimeout - SlowStartupThreshold]);
    }

    #endregion
}