namespace Lanceur.Core.Configuration.Sections.Application;

public class ProcessLaunchSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the delay (in milliseconds) to wait, once an alias is ready, before checking that it is
    ///     still running. The check also takes into account the processes with the same name that were already
    ///     running before the launch, so that an application handing over to an existing instance
    ///     (single-instance application) is not considered as closed.
    /// </summary>
    public int EarlyExitCheckDelay { get; set; } = 5_000;

    /// <summary>
    ///     Gets or sets the delay (in milliseconds) after which the startup of an alias is considered slow.
    ///     Once this delay is exceeded, a notification is displayed until the process is ready.
    /// </summary>
    public int SlowStartupMs { get; set; } = 2_000;

    /// <summary>
    ///     Gets or sets the maximum time (in milliseconds) to wait for an alias to be ready after it has started.
    /// </summary>
    public int StartupTimeout { get; set; } = 30_000;

    #endregion
}