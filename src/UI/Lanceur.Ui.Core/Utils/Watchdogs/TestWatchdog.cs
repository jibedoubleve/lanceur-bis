namespace Lanceur.Ui.Core.Utils.Watchdogs;

public class TestWatchdog : IWatchdog
{
    private readonly Func<Task>? _action;

    public TestWatchdog(Func<Task>? action) => _action = action;

    /// <inheritdoc />
    public async Task Pulse() => await _action?.Invoke()!;
}