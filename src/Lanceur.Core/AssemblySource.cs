using System.Reflection;

namespace Lanceur.Core;

/// <summary>
/// Represents a source for assemblies that contain macros and reserved keywords.
/// This class provides access to two specific assemblies: macros, reserved keywords...
/// </summary>
public class AssemblySource
{
    #region Fields

    private Assembly _macroSourceAsm;
    private Assembly _reservedKeywordsAsm;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the assembly containing all macro definitions for the application.
    /// If not explicitly set, defaults to the currently executing assembly.
    /// </summary>
    public Assembly MacroSource
    {
        get => _macroSourceAsm ?? Assembly.GetExecutingAssembly();
        init => _macroSourceAsm = value;
    }

    /// <summary>
    /// Gets the assembly containing all definitions for reserved keywords used by the application.
    /// If not explicitly set, defaults to the currently executing assembly.
    /// </summary>
    public Assembly ReservedKeywordSource
    {
        get => _reservedKeywordsAsm ?? Assembly.GetExecutingAssembly();
        init => _reservedKeywordsAsm = value;
    }

    #endregion
}