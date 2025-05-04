using CommunityToolkit.Mvvm.ComponentModel;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class NumericSelectorViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _label = "Numeric value:";
    [ObservableProperty] private double _maximum;
    [ObservableProperty] private double _minimum;
    [ObservableProperty] private double _numericValue;
    [ObservableProperty] private string _toolTip = "";

    #endregion
}