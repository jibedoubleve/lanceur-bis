using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.Views.Controls;

public partial class AdditionalParameterView
{
    #region Constructors

    public AdditionalParameterView(AdditionalParameter viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}