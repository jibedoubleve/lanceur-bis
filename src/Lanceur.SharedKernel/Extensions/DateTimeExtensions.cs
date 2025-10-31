namespace Lanceur.SharedKernel.Extensions;

public static class DateTimeExtensions
{
    #region Methods

    /// <summary>
    ///     Generates a list of all calendar days in the month of the specified date.
    /// </summary>
    /// <param name="date">
    ///     The reference date. If <see langword="null"/>, the method returns an empty list.
    /// </param>
    /// <returns>
    ///     A list of <see cref="DateTime"/> values, one for each day of the month corresponding to <paramref name="date"/>.
    ///     Returns an empty list if <paramref name="date"/> is <see langword="null"/>.
    /// </returns>
    public static IList<DateTime> GetDaysOfMonth(this DateTime? date)
    {
        if (date is null) return [];

        var year = date.Value.Year;
        var month = date.Value.Month;
        var dayInMonth = DateTime.DaysInMonth(year, month);

        var days = new List<DateTime>();
        for (var day = 1; day <= dayInMonth; day++)
            days.Add(
                new(year, month, day, 0, 0, 0, date.Value.Kind)
            );

        return days;
    }

    #endregion
}