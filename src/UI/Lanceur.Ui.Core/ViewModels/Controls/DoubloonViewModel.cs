using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class DoubloonViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private List<AdditionalParameter> _list;
    [ObservableProperty] private string _synonyms;

    #endregion

    #region Constructors

    public DoubloonViewModel(List<AdditionalParameter> list, string synonyms)
    {
        _list = list;
        _synonyms = synonyms;
    }

    #endregion
}