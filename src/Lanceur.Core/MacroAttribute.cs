namespace Lanceur.Core;

[AttributeUsage(AttributeTargets.Class)]
public class MacroAttribute : Attribute
{
    #region Fields

    private readonly string _name;

    #endregion Fields

    #region Constructors

    public MacroAttribute(string name, bool isVisible = true)
    {
        IsVisible = isVisible;
        _name = name.Trim().Replace("@", "").ToUpper();
    }

    #endregion Constructors

    #region Properties

    /// <summary>
    /// Indicates whether this macro should appear in the list of macros
    /// This is meant to create some macro for "privileged" people. That's
    /// some debugging tools for the developer.
    /// </summary>
    public bool IsVisible { get; }

    public string Name => $"@{_name}@";

    #endregion Properties
}