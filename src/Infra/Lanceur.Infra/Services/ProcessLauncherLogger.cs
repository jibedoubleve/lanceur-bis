using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
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
        "Executing process with context: {ProcessContext}",
        context.ToJson()
    );
    
    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> Start(ISelfExecutable executable, Cmdline cmdline)
    {
        _logger.LogInformation("Execute self executable: {Json}", executable.ToJson());
        return await Task.FromResult(new List<QueryResult>());
    }

    #endregion
}