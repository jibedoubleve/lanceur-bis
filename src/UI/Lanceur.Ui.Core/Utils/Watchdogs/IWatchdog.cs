namespace Lanceur.Ui.Core.Utils.Watchdogs;

/// <summary>
///     Implements a watchdog timer to execute an action after a countdown.
///     If restarted before the countdown ends, the timer resets.
/// </summary>
public interface IWatchdog
{
    /// <summary>
    ///     Starts the watchdog timer. If already running, it stops and resets the timer.
    /// </summary>
    Task Pulse();
}