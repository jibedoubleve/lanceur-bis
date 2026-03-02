namespace Lanceur.Ui.Core.Utils.Watchdogs;

/// <summary>
///     Represents a builder interface for constructing Watchdog instances with customizable configurations.
/// </summary>
public interface IWatchdogBuilder
{
    #region Methods

    /// <summary>
    ///     Constructs and returns a fully configured <see cref="IWatchdog" /> instance based on the specified configurations.
    /// </summary>
    /// <returns>A fully configured instance of <see cref="IWatchdog" />.</returns>
    IWatchdog Build();

    /// <summary>
    ///     Specifies the action to execute when the watchdog triggers.
    ///     This defines the behaviour to occur when the watchdog conditions are met.
    /// </summary>
    /// <param name="action">An asynchronous function to execute when the watchdog triggers.</param>
    /// <returns>The current instance of <see cref="IWatchdogBuilder" /> to allow method chaining.</returns>
    IWatchdogBuilder WithAction(Func<Task> action);

    /// <summary>
    ///     Configures the interval for the watchdog, specifying how frequently the watchdog triggers.
    /// </summary>
    /// <param name="interval">The time interval between each trigger of the watchdog.</param>
    /// <returns>The current instance of <see cref="IWatchdogBuilder" /> to allow method chaining.</returns>
    IWatchdogBuilder WithInterval(TimeSpan interval);

    #endregion
}