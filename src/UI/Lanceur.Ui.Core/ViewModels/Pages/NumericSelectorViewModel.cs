using CommunityToolkit.Mvvm.ComponentModel;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class NumericSelectorViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _label = "Numeric value:";
    [ObservableProperty] private double _maximum;
    [ObservableProperty] private double _minimum;
    [ObservableProperty] private string _toolTip = "";
    [ObservableProperty] private double _numericValue;

    #endregion
}