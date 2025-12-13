using Humanizer;
using System.Windows.Threading;

namespace Lanceur.Ui.Core.Utils.Watchdogs;

public class Watchdog : IWatchdog
{
    private readonly DispatcherTimer _timer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Watchdog" /> class with a countdown duration and an action to execute.
    /// </summary>
    /// <param name="interval">The duration of the interval in milliseconds.</param>
    /// <param name="action">The action to execute when the countdown reaches zero.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="action" /> parameter is null.</exception>
    public Watchdog(Func<Task> action, TimeSpan interval)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        _timer = new() { Interval = interval };
        _timer.Tick += async (_, _) =>
        {
            _timer.Stop();
            await action();
        };
    }

    /// <inheritdoc />
    public Task Pulse()
    {
        if (_timer.IsEnabled) _timer.Stop();
        _timer.Start();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void ResetDelay(double searchDelay) => _timer.Interval = searchDelay.Milliseconds();
}