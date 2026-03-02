namespace Lanceur.Ui.Core.Utils.Watchdogs;

public class TestWatchdogBuilder : IWatchdogBuilder
{
    #region Fields

    private Func<Task>? _action;

    #endregion

    #region Methods

    public IWatchdog Build() => new TestWatchdog(_action);

    public IWatchdogBuilder WithAction(Func<Task> action)
    {
        _action = action;
        return this;
    }

    public IWatchdogBuilder WithInterval(TimeSpan interval) => this;

    #endregion
}