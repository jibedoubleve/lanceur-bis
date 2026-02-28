namespace Lanceur.Core;

[AttributeUsage(AttributeTargets.Class)]
public class MacroAttribute : Attribute
{
    #region Fields

    #endregion

    #region Constructors

    public MacroAttribute(string name, bool isVisible = true)
    {
        IsVisible = isVisible;
        Name = name.Trim().Replace("@", "").ToUpper();
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Indicates whether this macro should appear in the list of macros
    ///     This is meant to create some macro for "privileged" people. That's
    ///     some debugging tools for the developer.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    ///     The name of the macro.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The macro signature as stored in <c>FileName</c>, surrounded by <c>@</c> delimiters (e.g. <c>@NAME@</c>).
    ///     This is used by the system to distinguish a macro from a regular alias.
    /// </summary>
    public string Signature => $"@{Name.ToUpper()}@";

    #endregion
}