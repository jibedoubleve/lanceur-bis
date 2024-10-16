using CommunityToolkit.Mvvm.ComponentModel;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class KeyValueListViewModel<TKey, TValue> : ObservableObject
{
    #region Fields

    [ObservableProperty] private List<KeyValueViewModel<TKey, TValue>> _list;
    [ObservableProperty] private string _synonyms;

    #endregion

    #region Constructors

    public KeyValueListViewModel(List<KeyValueViewModel<TKey, TValue>> list, string synonyms)
    {
        _list = list;
        _synonyms = synonyms;
    }

    #endregion
}