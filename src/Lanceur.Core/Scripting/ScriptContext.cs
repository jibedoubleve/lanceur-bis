namespace Lanceur.Core.Scripting;

/// <summary>
/// Represents the execution context for a script, containing metadata and state information.
/// </summary>
public class ScriptContext
{
    #region Properties

    /// <summary>
    /// Gets an empty instance of <see cref="ScriptContext"/>.
    /// </summary>
    public static ScriptContext Empty => new();

    /// <summary>
    /// Gets or sets the file name associated with the alias.
    /// </summary>
    /// <remarks>
    /// This property should be set through the scripting facility rather than directly.
    /// </remarks>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the script execution has been cancelled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the script has been cancelled; otherwise, <c>false</c>.
    /// </value>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Gets or sets the parameters passed to the alias.
    /// </summary>
    /// <remarks>
    /// This property should be set through the scripting facility rather than directly.
    /// </remarks>
    public string Parameters { get; set; } = string.Empty;

    #endregion
}