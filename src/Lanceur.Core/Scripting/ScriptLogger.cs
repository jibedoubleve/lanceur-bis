using System.Text;

namespace Lanceur.Core.Scripting;

/// <summary>
///     Provides timestamped logging to a file on the desktop.
/// </summary>
public class ScriptLogger
{
    #region Fields

    private readonly StringBuilder _logs = new();

    private const string LogTemplate = "{0}: {1}";

    #endregion

    #region Properties

    /// <summary>
    ///     Indicates whether there'are logs to be flushed
    /// </summary>
    public bool IsEmpty { get; private set; } = true;

    #endregion

    #region Methods

    /// <summary>
    ///     Appends a timestamped line to the log buffer.
    /// </summary>
    /// <param name="line">The text to log.</param>
    public void AppendLine(string line)
    {
        if(IsEmpty) 
        {
            _logs.AppendLine("==================NEW EXECUTION==================");
        }
        IsEmpty = false;
        _logs.AppendLine(string.Format(LogTemplate, DateTime.Now.ToString("o"), line));
    }

    /// <summary>
    ///     Writes all buffered logs to a file on the desktop. Logs are not persisted until this method is called.
    /// </summary>
    public void Flush()
    {
        var content = _logs.ToString();
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var output = Path.Combine(desktop, "script_debug.log");
        File.AppendAllText(output, content);
    }

    #endregion
}