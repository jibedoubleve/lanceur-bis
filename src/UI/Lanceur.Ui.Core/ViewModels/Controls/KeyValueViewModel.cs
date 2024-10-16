using CommunityToolkit.Mvvm.ComponentModel;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class KeyValueViewModel<TKey, TValue> : ObservableObject
{
    #region Fields

    [ObservableProperty] private TKey _key;
    [ObservableProperty] private TValue _value;

    #endregion

    #region Constructors

    public KeyValueViewModel(TKey key, TValue value)
    {
        _key = key;
        _value = value;
    }

    #endregion
}