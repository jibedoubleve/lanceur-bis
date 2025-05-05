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

    #endregion
}

public class ProcessContext
{
    #region Properties

    public string Arguments { get; set; }
    public string FileName { get; set; }
    public SharedKernel.Constants.StartMode WindowStyle { get; set; }
    public bool UseShellExecute { get; set; }
    public string Verb { get; set; }
    public string WorkingDirectory { get; set; }

    #endregion
}