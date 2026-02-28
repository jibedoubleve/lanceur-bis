using Lanceur.SharedKernel.Utils;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Extensions;

public static class WpfUiExtensions
{
    #region Methods

    public static WindowBackdropType ToWindowBackdropType(this string value)
    {
        if (string.Equals(value, nameof(WindowBackdropType.Acrylic), StringComparison.CurrentCultureIgnoreCase))
        {
            return WindowBackdropType.Acrylic;
        }

        if (string.Equals(value, nameof(WindowBackdropType.None), StringComparison.CurrentCultureIgnoreCase))
        {
            return WindowBackdropType.None;
        }

        if (string.Equals(value, nameof(WindowBackdropType.Auto), StringComparison.CurrentCultureIgnoreCase))
        {
            return WindowBackdropType.Auto;
        }

        if (string.Equals(value, nameof(WindowBackdropType.Mica), StringComparison.CurrentCultureIgnoreCase))
        {
            return WindowBackdropType.Mica;
        }

        if (string.Equals(value, nameof(WindowBackdropType.Tabbed), StringComparison.CurrentCultureIgnoreCase))
        {
            return WindowBackdropType.Tabbed;
        }

        return ConditionalExecution.Execute(
            () => throw new NotSupportedException($"{value} is not a wpf backdrop type."),
            () => WindowBackdropType.Auto
        );
    }

    #endregion
}