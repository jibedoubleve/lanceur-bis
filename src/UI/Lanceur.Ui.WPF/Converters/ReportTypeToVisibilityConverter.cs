using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Converters;

public enum ActionOnAlias
{
    UpdateDescription,
    Delete,
    Merge,
    Restore,
    DeletePermanently,
    InactivitySelector,
    LowUsageSelector
}

public class ReportTypeToVisibilityConverter : IValueConverter
{
    #region Methods

    private Visibility GetDefaultVisibility(ActionOnAlias actionOnAlias)
    {
        return actionOnAlias switch
        {
            ActionOnAlias.UpdateDescription  => Visibility.Collapsed,
            ActionOnAlias.Delete             => Visibility.Visible,
            ActionOnAlias.Merge              => Visibility.Collapsed,
            ActionOnAlias.Restore            => Visibility.Collapsed,
            ActionOnAlias.DeletePermanently  => Visibility.Collapsed,
            ActionOnAlias.InactivitySelector => Visibility.Collapsed,
            ActionOnAlias.LowUsageSelector   => Visibility.Collapsed,
            _                                => throw new ArgumentOutOfRangeException(nameof(actionOnAlias), actionOnAlias, null)
        };
    }

    private Visibility GetVisibilityForDeleted(ActionOnAlias actionOnAlias)
    {
        return actionOnAlias switch
        {
            ActionOnAlias.UpdateDescription  => Visibility.Collapsed,
            ActionOnAlias.Delete             => Visibility.Collapsed,
            ActionOnAlias.Merge              => Visibility.Collapsed,
            ActionOnAlias.Restore            => Visibility.Visible,
            ActionOnAlias.DeletePermanently  => Visibility.Visible,
            ActionOnAlias.InactivitySelector => Visibility.Collapsed,
            ActionOnAlias.LowUsageSelector   => Visibility.Collapsed,
            _                                => throw new ArgumentOutOfRangeException(nameof(actionOnAlias), actionOnAlias, null)
        };
    }

    private static Visibility GetVisibilityForDoubloon(ActionOnAlias actionOnAlias)
    {
        return actionOnAlias switch
        {
            ActionOnAlias.UpdateDescription  => Visibility.Collapsed,
            ActionOnAlias.Delete             => Visibility.Visible,
            ActionOnAlias.Merge              => Visibility.Visible,
            ActionOnAlias.Restore            => Visibility.Collapsed,
            ActionOnAlias.DeletePermanently  => Visibility.Collapsed,
            ActionOnAlias.InactivitySelector => Visibility.Collapsed,
            ActionOnAlias.LowUsageSelector   => Visibility.Collapsed,
            _                                => throw new ArgumentOutOfRangeException(nameof(actionOnAlias), actionOnAlias, null)
        };
    }

    private object GetVisibilityForInactive(ActionOnAlias actionOnAlias)
    {
        return actionOnAlias switch
        {
            ActionOnAlias.UpdateDescription  => Visibility.Collapsed,
            ActionOnAlias.Delete             => Visibility.Visible,
            ActionOnAlias.Merge              => Visibility.Collapsed,
            ActionOnAlias.Restore            => Visibility.Collapsed,
            ActionOnAlias.DeletePermanently  => Visibility.Collapsed,
            ActionOnAlias.InactivitySelector => Visibility.Visible,
            ActionOnAlias.LowUsageSelector   => Visibility.Collapsed,
            _                                => throw new ArgumentOutOfRangeException(nameof(actionOnAlias), actionOnAlias, null)
        };
    }

    private Visibility GetVisibilityForUnannotated(ActionOnAlias actionOnAlias)
    {
        return actionOnAlias switch
        {
            ActionOnAlias.UpdateDescription  => Visibility.Visible,
            ActionOnAlias.Delete             => Visibility.Collapsed,
            ActionOnAlias.Merge              => Visibility.Collapsed,
            ActionOnAlias.Restore            => Visibility.Collapsed,
            ActionOnAlias.DeletePermanently  => Visibility.Collapsed,
            ActionOnAlias.InactivitySelector => Visibility.Collapsed,
            ActionOnAlias.LowUsageSelector   => Visibility.Collapsed,
            _                                => throw new ArgumentOutOfRangeException(nameof(actionOnAlias), actionOnAlias, null)
        };
    }

    private Visibility GetVisibilityForLowUsage(ActionOnAlias actionOnAlias)
    {
        return actionOnAlias switch
        {
            ActionOnAlias.UpdateDescription  => Visibility.Collapsed,
            ActionOnAlias.Delete             => Visibility.Visible,
            ActionOnAlias.Merge              => Visibility.Collapsed,
            ActionOnAlias.Restore            => Visibility.Collapsed,
            ActionOnAlias.DeletePermanently  => Visibility.Collapsed,
            ActionOnAlias.InactivitySelector => Visibility.Collapsed,
            ActionOnAlias.LowUsageSelector   => Visibility.Visible,
            _                                => throw new ArgumentOutOfRangeException(nameof(actionOnAlias), actionOnAlias, null)
        };
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ReportType report) return Binding.DoNothing;
        if (parameter is not string paramString) return Binding.DoNothing;

        if (false == Enum.TryParse(paramString, out ActionOnAlias actionOnAlias)) throw new InvalidCastException($"Cannot cast '{paramString}' to 'ActionOnAlias'");

        return report switch
        {
            ReportType.DoubloonAliases    => GetVisibilityForDoubloon(actionOnAlias),
            ReportType.UnannotatedAliases => GetVisibilityForUnannotated(actionOnAlias),
            ReportType.RestoreAlias       => GetVisibilityForDeleted(actionOnAlias),
            ReportType.InactiveAliases    => GetVisibilityForInactive(actionOnAlias),
            ReportType.RarelyUsedAliases    => GetVisibilityForLowUsage(actionOnAlias),
            ReportType.None               => Visibility.Visible,
            _                             => GetDefaultVisibility(actionOnAlias)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    #endregion
}