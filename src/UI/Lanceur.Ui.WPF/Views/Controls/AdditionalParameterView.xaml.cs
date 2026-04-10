using Lanceur.Core.Models;
using Lanceur.SharedKernel.IoC;

namespace Lanceur.Ui.WPF.Views.Controls;

[ExcludeFromIoC]
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