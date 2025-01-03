using Lanceur.SharedKernel.Utils;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lanceur.Ui.WPF.Converters
{
    internal class DebugToVisibilityConverter : IValueConverter
    {
        #region Fields

        private Conditional<Visibility> _conditional = new(Visibility.Visible, Visibility.Collapsed);

        #endregion Fields

        #region Methods

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is Visibility ? _conditional.Value : Binding.DoNothing;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

        #endregion Methods

    }
}
