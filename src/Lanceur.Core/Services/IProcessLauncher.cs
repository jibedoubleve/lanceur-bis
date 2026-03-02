using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

public interface IProcessLauncher
{
    #region Methods

    /// <summary>
    ///     Opens the specified file using the default application registered on the system
    ///     for the file type, as determined by the file extension.
    /// </summary>
    /// <param name="path">
    ///     The absolute path to the file to open. Must not be null or empty.
    /// </param>
    void Open(string path);

    /// <summary>
    ///     Starts a new process based on the specified <see cref="ProcessContext" />.
    ///     This may include details such as the executable path, arguments,
    ///     working directory, and environment variables.
    /// </summary>
    /// <param name="context">
    ///     An instance of <see cref="ProcessContext" /> containing the configuration
    ///     required to start the process. Must not be null.
    /// </param>
    void Start(ProcessContext context);

    /// <summary>
    ///     Starts the process defined by the specified <see cref="ISelfExecutable" /> instance,
    ///     using the provided <see cref="Cmdline" /> as execution parameters.
    /// </summary>
    /// <param name="executable">
    ///     The <see cref="ISelfExecutable" /> instance representing the process to start.
    ///     Must not be null.
    /// </param>
    /// <param name="cmdline">
    ///     The <see cref="Cmdline" /> instance containing the command-line parameters
    ///     to use for the process execution. Must not be null.
    /// </param>
    Task<IEnumerable<QueryResult>> Start(ISelfExecutable executable, Cmdline cmdline);

    #endregion
}

public class ProcessContext
{
    #region Properties

    public string Arguments { get; set; }
    public string FileName { get; set; }
    public bool UseShellExecute { get; set; }
    public string Verb { get; set; }
    public SharedKernel.Constants.StartMode WindowStyle { get; set; }
    public string WorkingDirectory { get; set; }

    #endregion
}