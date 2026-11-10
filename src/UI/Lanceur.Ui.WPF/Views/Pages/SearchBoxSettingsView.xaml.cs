using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class SearchBoxSettingsView
{
    #region Constructors

    public SearchBoxSettingsView(ApplicationSettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}
