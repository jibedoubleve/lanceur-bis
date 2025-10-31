namespace Lanceur.SharedKernel.Extensions;

public static class DateTimeExtensions
{
    #region Methods

    /// <summary>
    ///     Returns a list containing all days of the month corresponding to the specified date.
    /// </summary>
    /// <param name="date">
    ///     The reference date. If null, an empty list is returned.
    /// </param>
    /// <returns>
    ///     A list of <see cref="DateTime" /> objects representing each day of the month.
    /// </returns>
    public static IList<DateTime> GetDaysOfMonth(this DateTime? date)
    {
        if (date is null) return [];

        var year = date.Value.Year;
        var month = date.Value.Month;
        var dayInMonth = DateTime.DaysInMonth(year, month);

        var days = new List<DateTime>();
        for (var d = 1; d <= dayInMonth; d++) days.Add(new(year, month, d));

        return days;
    }

    #endregion
}