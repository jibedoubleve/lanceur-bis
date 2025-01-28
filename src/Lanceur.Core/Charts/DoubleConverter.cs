using System.Globalization;

namespace Lanceur.Core.Charts;

/// <summary>
///     Provides methods for converting a <see cref="double" /> value to various destination types.
/// </summary>
public static class DoubleConverter
{
    #region Methods

    /// <summary>
    ///     Converts a numeric representation of the day of the week (1 through 7) to its corresponding day name.
    /// </summary>
    /// <param name="dayOfWeek">A double representing the day of the week, where 1 = Monday and 7 = Sunday.</param>
    /// <returns>A string representing the name of the day of the week.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dayOfWeek" /> is outside the range 1 to 7.</exception>
    public static string ToDayOfWeek(this double dayOfWeek) => dayOfWeek switch
    {
        0 => "Monday",
        1 => "Tuesday",
        2 => "Wednesday",
        3 => "Thursday",
        4 => "Friday",
        5 => "Saturday",
        6 => "Sunday",
        _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
    };

    /// <summary>
    ///     Converts a double representing an OLE Automation date into a string formatted as the full month name and year.
    /// </summary>
    /// <param name="time">A double representing the OLE Automation date.</param>
    /// <returns>A string representing the full month name and year of the input date.</returns>
    public static string ToMonthYear(this double time)
    {
        var culture = new CultureInfo("en-US");
        return DateTime.FromOADate(time).ToString("MMM yy", culture);
    }


    /// <summary>
    ///     Converts a double representing a time in hours (with minutes as the decimal part) into a string formatted as
    ///     "HH:MM".
    /// </summary>
    /// <param name="time">A double representing the time in hours. For example, 14.5 represents 14 hours and 30 minutes.</param>
    /// <returns>A string in the format "HH:MM" representing the input time.</returns>
    public static string ToTimeString(this double time) => TimeSpan.FromHours(time)
                                                                   .ToString(@"hh\:mm");

    /// <summary>
    ///     Converts a double representing an OLE Automation date into a string formatted as the full year.
    /// </summary>
    /// <param name="time">A double representing the OLE Automation date.</param>
    /// <returns>A string representing the full year of the input date.</returns>
    public static string ToYear(this double time)
    {
        var culture = new CultureInfo("en-US");
        return DateTime.FromOADate(time).ToString("yyyy", culture);
    }

    #endregion
}