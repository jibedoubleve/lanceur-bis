namespace Lanceur.Core.Configuration.Sections;

public class ScriptingSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the language used for scripting.
    ///     This is a global configuration; Lua and C# scripting cannot be mixed—it must be one or the other.
    /// </summary>
    public ScriptLanguage ScriptLanguage { get; set; } = ScriptLanguage.Lua;

    /// <summary>
    ///     Gets or sets the namespaces to import into the script context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This property is only applicable when using the C# scripting engine and is ignored for Lua scripts.
    ///     </para>
    ///     <para>
    ///         This setting should not be exposed in the user interface because invalid namespace references
    ///         will cause runtime failures without compile-time validation.
    ///     </para>
    /// </remarks>
    /// <value>
    ///     A collection of namespace strings to import. Defaults to System and System.IO.
    /// </value>
    public IEnumerable<string> Usings { get; set; } = ["System", "System.IO"];

    #endregion
}