namespace Lanceur.Core.Stores;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StoreAttribute : Attribute
{
    #region Constructors

    public StoreAttribute() { }

    public StoreAttribute(string defaultShortcut)
        => DefaultShortcut = defaultShortcut;

    #endregion

    #region Properties

    public string DefaultShortcut { get; } = string.Empty;

    #endregion
}