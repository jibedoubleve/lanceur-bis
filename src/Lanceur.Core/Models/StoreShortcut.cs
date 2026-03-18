namespace Lanceur.Core.Models;

public sealed class StoreShortcut : ObservableModel
{

    #region Properties

    /// <summary>
    ///     Overrides the default "alive pattern" for the store.
    ///     This acts as a shortcut for setting a custom pattern.
    /// </summary>
    public string? AliasOverride
    {
        get;
        set => SetField(ref field, value);
    }

    /// <summary>
    ///     Key to help to find the store. This is the result of a GetType().ToString()
    /// </summary>
    public string? StoreType
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion
}