using System.Globalization;
using System.Windows.Data;
using Lanceur.Core.Constants;

namespace Lanceur.Ui.WPF.Converters;

public class ReportTypeToTextConverter : IValueConverter
{
    #region Methods

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ReportType reportType)
            return reportType switch
            {
                ReportType.None               => "Use default settings",
                ReportType.DoubloonAliases    => "Doubloons",
                ReportType.BrokenAliases      => "Broken aliases",
                ReportType.UnannotatedAliases => "Aliases without comments",
                ReportType.RestoreAlias       => "Deleted aliases",
                ReportType.UnusedAliases      => "Never used aliases",
                ReportType.InactiveAliases    => "Inactive aliases",
                ReportType.RarelyUsedAliases  => "Rarely used aliases report",
                _                             => "Unrecognised report type"
            };

        return Binding.DoNothing;
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    #endregion
}