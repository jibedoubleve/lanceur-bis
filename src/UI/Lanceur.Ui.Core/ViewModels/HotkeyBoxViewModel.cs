using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lanceur.Ui.Core.ViewModels;

public partial class HotkeyBoxViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private bool _isAlt;
    [ObservableProperty] private bool _isCtrl;
    [ObservableProperty] private bool _isShift;
    [ObservableProperty] private bool _isWin;
    [ObservableProperty] private int _key;

    #endregion

    #region Properties

    public ModifierKeys ModifierKeys
    {
        get
        {
            var modifierKeys = ModifierKeys.None;
            if (IsAlt) modifierKeys |= ModifierKeys.Alt;
            if (IsCtrl) modifierKeys |= ModifierKeys.Control;
            if (IsShift) modifierKeys |= ModifierKeys.Shift;
            if (IsWin) modifierKeys |= ModifierKeys.Windows;
            return modifierKeys;
        }
    }

    #endregion
}