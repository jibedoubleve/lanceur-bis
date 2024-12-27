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
    
    /// <summary>
    /// Replace the value of the interval of the watchdog with the specified value
    /// </summary>
    /// <param name="searchDelay">The new delay to apply in milliseconds</param>
    void ResetDelay(double searchDelay);
}