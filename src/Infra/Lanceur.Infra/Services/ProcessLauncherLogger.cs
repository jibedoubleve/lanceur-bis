using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class ProcessLauncherLogger : IProcessLauncher
{
    #region Fields

    private readonly ILogger<ProcessLauncherLogger> _logger;

    #endregion

    #region Constructors

    public ProcessLauncherLogger(ILogger<ProcessLauncherLogger> logger) => _logger = logger;

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Open(string path) => _logger.LogInformation("Opening {Path}", path);

    /// <inheritdoc />
    public void Start(ProcessContext context) => _logger.LogInformation(
        "Executing process with context: {@ProcessContext}",
        context
    );

    #endregion
}