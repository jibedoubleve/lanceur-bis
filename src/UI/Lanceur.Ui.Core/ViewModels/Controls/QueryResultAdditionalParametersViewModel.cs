using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class QueryResultAdditionalParametersViewModel : ObservableObject
{
    #region Fields

    private readonly AdditionalParameter _model;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _parameter = string.Empty;

    #endregion

    #region Constructors

    public QueryResultAdditionalParametersViewModel(AdditionalParameter model) => _model = model;

    #endregion
}