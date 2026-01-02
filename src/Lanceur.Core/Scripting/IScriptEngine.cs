using Lanceur.Core.Configuration.Sections;

namespace Lanceur.Core.Scripting;

/// <summary>
///     Defines the contract for executing scripts in different scripting languages.
/// </summary>
public interface IScriptEngine
{
    #region Properties

    /// <summary>
    ///     Gets the scripting language supported by this engine.
    /// </summary>
    /// <value>
    ///     The <see cref="ScriptLanguage" /> enumeration value representing the supported language (e.g., C#, Lua).
    /// </value>
    ScriptLanguage Language { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Asynchronously executes the specified script.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    /// <param name="isDebug">
    ///     <see langword="true" /> to enable debug mode and flush all logs to a file;
    ///     <see langword="false" /> to disable logging. The default is <see langword="false" />.
    /// </param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> representing the asynchronous operation,
    ///     containing a <see cref="ScriptResult" /> with the execution result.
    /// </returns>
    Task<ScriptResult> ExecuteScriptAsync(Script script, bool isDebug = false);

    #endregion
}