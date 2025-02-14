using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Ui.Core.ViewModels;

public partial class ExceptionViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _windowBackdropStyle;

    #endregion

    #region Constructors

    public ExceptionViewModel(ISettingsFacade settingsFacade) => WindowBackdropStyle = settingsFacade.Application.Window.BackdropStyle;

    #endregion
}