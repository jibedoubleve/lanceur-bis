using Lanceur.SharedKernel.Utils;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Extensions;

public static class WpfUiExtensions
{
    #region Methods

    public static WindowBackdropType ToWindowBackdropType(this string value)
    {
        if (string.Equals(value, WindowBackdropType.Acrylic.ToString(), StringComparison.CurrentCultureIgnoreCase)) return WindowBackdropType.Acrylic;
        if (string.Equals(value, WindowBackdropType.None.ToString(), StringComparison.CurrentCultureIgnoreCase)) return WindowBackdropType.None;
        if (string.Equals(value, WindowBackdropType.Auto.ToString(), StringComparison.CurrentCultureIgnoreCase)) return WindowBackdropType.Auto;
        if (string.Equals(value, WindowBackdropType.Mica.ToString(), StringComparison.CurrentCultureIgnoreCase)) return WindowBackdropType.Mica;
        if (string.Equals(value, WindowBackdropType.Tabbed.ToString(), StringComparison.CurrentCultureIgnoreCase)) return WindowBackdropType.Tabbed;

        return ConditionalExecution.Execute(
            () => throw new NotSupportedException($"{value} is not a wpf backdrop type."),
            () => WindowBackdropType.Auto
        );
    }

    #endregion
}