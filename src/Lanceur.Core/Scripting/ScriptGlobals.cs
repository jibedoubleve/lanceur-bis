namespace Lanceur.Core.Scripting;

/// <summary>
/// Globals that are available to C# scripts
/// </summary>
public class ScriptGlobals
{
    /// <summary>
    /// The context information for the current script execution
    /// </summary>
    public ScriptContext Context { get; set; } = ScriptContext.Empty;

    /// <summary>
    /// Notification service to display messages to the user
    /// </summary>
    public NotificationScriptAdapter Notification { get; set; } = null!;
}
