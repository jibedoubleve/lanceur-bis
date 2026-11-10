using System.Text;

namespace Lanceur.SharedKernel.Utils;

/// <summary>
///     Captures timestamped log output into an in-memory buffer.
/// </summary>
public sealed class TimestampedLogBuffer
{
    #region Fields

    private const string LogTemplate = "{0}: {1}";

    private readonly StringBuilder _logs = new();

    #endregion

    #region Methods

    /// <summary>
    ///     Appends a line prefixed with an ISO 8601 timestamp to the log buffer.
    /// </summary>
    /// <param name="line">The text to append.</param>
    public void AppendLine(string line) =>
        _logs.AppendLine(string.Format(LogTemplate, DateTime.Now.ToString("o"), line));

    /// <inheritdoc />
    public override string ToString() => _logs.ToString();

    #endregion
}