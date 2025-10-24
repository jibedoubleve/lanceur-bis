using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Services;

public class ProcessLauncherVisitor : IProcessLauncher
{
    #region Fields

    private readonly Action<string> _pathVisitor;
    private readonly Action<ProcessContext> _processContextVisitor;

    #endregion

    #region Constructors

    public ProcessLauncherVisitor(
        Action<ProcessContext> processContextVisitor = null,
        Action<string> pathVisitor = null
    )
    {
        _processContextVisitor = processContextVisitor;
        _pathVisitor = pathVisitor;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Open(string path) => _pathVisitor?.Invoke(path);

    /// <inheritdoc />
    public void Start(ProcessContext context) => _processContextVisitor?.Invoke(context);

    /// <inheritdoc />
    public async Task<IEnumerable<QueryResult>> Start(ISelfExecutable executable, Cmdline cmdline) 
        => await Task.FromResult(new List<QueryResult>());

    #endregion
}