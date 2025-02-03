using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Ui.Core.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _windowBackdropStyle;

    #endregion

    #region Constructors

    public SettingsViewModel(ISettingsFacade settingsFacade) => WindowBackdropStyle = settingsFacade.Application.Window.BackdropStyle;

    #endregion
}