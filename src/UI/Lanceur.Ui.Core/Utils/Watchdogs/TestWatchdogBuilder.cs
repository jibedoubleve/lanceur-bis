namespace Lanceur.Ui.Core.Utils.Watchdogs;

public class TestWatchdogBuilder : IWatchdogBuilder
{
    private Func<Task>? _action;
    public IWatchdogBuilder WithInterval(TimeSpan interval) => this;

    public IWatchdogBuilder WithAction(Func<Task> action)
    {
        _action = action;
        return this;
    }

    public IWatchdog Build() => new TestWatchdog(_action);
}