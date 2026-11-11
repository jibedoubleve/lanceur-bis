namespace Lanceur.Ui.Core.ViewModels.Pages;

/// <summary>
///     Alias-usage history of a month, as displayed by the usage calendar.
/// </summary>
/// <param name="DefaultDate">
///     The date to select when the month is displayed: today if it is the most recent day with history;
///     otherwise the earliest day with history. <c>null</c> when the month has no history.
/// </param>
/// <param name="Days">The days of the month on which at least one alias was used.</param>
public sealed record MonthHistory(DateTime? DefaultDate, IEnumerable<DateTime> Days);
