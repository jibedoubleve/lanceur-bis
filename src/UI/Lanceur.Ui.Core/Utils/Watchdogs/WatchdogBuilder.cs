namespace Lanceur.Ui.Core.Utils.Watchdogs;

public sealed class WatchdogBuilder : IWatchdogBuilder
{
    #region Fields

    private Func<Task>? _action;
    private TimeSpan _interval;

    #endregion

    #region Methods

    public IWatchdog Build() =>
        _action is null
            ? throw new ArgumentException(nameof(_action))
            : new Watchdog(_action, _interval);

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

    #endregion
}