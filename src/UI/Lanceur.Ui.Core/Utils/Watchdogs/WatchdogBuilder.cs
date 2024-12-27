namespace Lanceur.Ui.Core.Utils.Watchdogs;

public class WatchdogBuilder : IWatchdogBuilder
{
    private Func<Task>? _action;
    private TimeSpan _interval;

    public IWatchdogBuilder WithAction(Func<Task> action)
    {
        _action = action;
        return this;
    }

    public IWatchdogBuilder WithInterval(TimeSpan interval)
    {
        _interval = interval;
        return this;
    }

    public IWatchdog Build()
    {
        if (_action is null) throw new ArgumentNullException(nameof(_action));

        return new Watchdog(_action, _interval);
    }
}