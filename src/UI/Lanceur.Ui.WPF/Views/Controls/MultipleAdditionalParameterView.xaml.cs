namespace Lanceur.Ui.WPF.Views.Controls;

/// <summary>
///     Interaction logic for MultipleAdditionnalParameterView.xaml
/// </summary>
public partial class MultipleAdditionalParameterView
{
    #region Constructors

    public MultipleAdditionalParameterView(object viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}