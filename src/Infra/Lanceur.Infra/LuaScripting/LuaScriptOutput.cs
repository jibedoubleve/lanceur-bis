using System.Text;

namespace Lanceur.Infra.LuaScripting;

/// <summary>
///     Provides timestamped logging to a file on the desktop.
/// </summary>
public class LuaScriptOutput
{
    #region Fields

    private readonly StringBuilder _logs = new();

    private const string LogTemplate = "{0}: {1}";

    #endregion

    #region Methods

    /// <summary>
    ///     Appends a timestamped line to the log buffer.
    /// </summary>
    /// <param name="line">The text to log.</param>
    public void AppendLine(string line)
    {
        _logs.AppendLine(string.Format(LogTemplate, DateTime.Now.ToString("o"), line));
    }

    /// <inheritdoc />
    public override string ToString() => _logs.ToString();

    #endregion
}