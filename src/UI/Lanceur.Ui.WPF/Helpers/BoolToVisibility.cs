using System.Windows;

namespace Lanceur.Ui.WPF.Helpers;

public class BoolToVisibility(bool value)
{
    #region Fields

    private readonly bool _value = value;

    #endregion

    #region Methods

    public static implicit operator Visibility(BoolToVisibility converter)
        => converter._value ? Visibility.Visible : Visibility.Collapsed;

    #endregion
}