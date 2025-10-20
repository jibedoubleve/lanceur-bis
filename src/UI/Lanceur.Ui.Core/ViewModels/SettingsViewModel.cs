using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;

namespace Lanceur.Ui.Core.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private string _windowBackdropStyle;

    #endregion

    #region Constructors

    public SettingsViewModel(ISection<WindowSection> settings) => WindowBackdropStyle = settings.Value.BackdropStyle;

    #endregion
}