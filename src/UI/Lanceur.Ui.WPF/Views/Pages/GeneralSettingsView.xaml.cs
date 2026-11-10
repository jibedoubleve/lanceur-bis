using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class GeneralSettingsView
{
    #region Constructors

    public GeneralSettingsView(ApplicationSettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}
