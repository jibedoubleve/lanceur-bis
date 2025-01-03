using CommunityToolkit.Mvvm.ComponentModel;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class MultipleAdditionalParameterViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _rawParameters = "";

    #endregion
}