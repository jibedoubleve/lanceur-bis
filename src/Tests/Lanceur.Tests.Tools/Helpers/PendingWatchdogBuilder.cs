using Lanceur.Ui.Core.Utils.Watchdogs;

namespace Lanceur.Tests.Tools.Helpers;

public sealed class PendingWatchdogBuilder : IWatchdog, IWatchdogBuilder
{
    private Func<Task> _action = () => Task.CompletedTask;
    private bool _isPending;

    public IWatchdog Build() => this;

    public IWatchdogBuilder WithAction(Func<Task> action)
    {
        _action = action;
        return this;
    }

    public IWatchdogBuilder WithInterval(TimeSpan interval) => this;

    public Task Pulse()
    {
        _isPending = true;
        return Task.CompletedTask;
    }

    public async Task Flush()
    {
        if (!_isPending) { return; }

        _isPending = false;
        await _action();
    }

    public void ResetDelay(double searchDelay) { }
}