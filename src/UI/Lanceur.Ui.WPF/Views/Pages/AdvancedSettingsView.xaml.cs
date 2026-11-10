using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class AdvancedSettingsView
{
    #region Constructors

    public AdvancedSettingsView(ApplicationSettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}
