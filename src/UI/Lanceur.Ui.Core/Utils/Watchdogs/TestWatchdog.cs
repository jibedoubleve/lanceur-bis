namespace Lanceur.Ui.Core.Utils.Watchdogs;

public class TestWatchdog : IWatchdog
{
    #region Fields

    private readonly Func<Task>? _action;

    #endregion

    #region Constructors

    public TestWatchdog(Func<Task>? action) => _action = action;

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task Pulse() => await _action?.Invoke()!;

    public void ResetDelay(double searchDelay)
    {
        /* Do nothing */
    }

    #endregion
}