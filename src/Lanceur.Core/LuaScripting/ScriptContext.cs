namespace Lanceur.Core.LuaScripting;

public class ScriptContext
{
    #region Properties

    public static ScriptContext Empty => new();
    public string FileName { get; init; } = string.Empty;


    /// <summary>
    ///     Gets or sets a value indicating whether the script has been cancelled.
    /// </summary>
    public bool IsCancelled { get; set; }

    public string Parameters { get; init; } = string.Empty;

    #endregion
}