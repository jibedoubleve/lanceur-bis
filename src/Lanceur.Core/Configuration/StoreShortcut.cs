using Lanceur.Core.Models;

namespace Lanceur.Core.Configuration;

public class StoreShortcut : ObservableModel
{
    #region Fields

    private string _storeOverride;
    private string _storeType;

    #endregion

    #region Properties

    /// <summary>
    ///     Overrides the default "alive pattern" for the store.
    ///     This acts as a shortcut for setting a custom pattern.
    /// </summary>
    public string AliasOverride
    {
        get => _storeOverride;
        set => SetField(ref _storeOverride, value);
    }

    /// <summary>
    ///     Key to help to find the store. This is the result of a GetType().ToString()
    /// </summary>
    public string StoreType
    {
        get => _storeType;
        set => SetField(ref _storeType, value);
    }

    #endregion
}