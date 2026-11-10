using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class AppearanceSettingsView
{
    #region Constructors

    public AppearanceSettingsView(ApplicationSettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}
