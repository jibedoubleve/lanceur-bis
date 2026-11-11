namespace Lanceur.Ui.Core.ViewModels.Pages;

public sealed record MonthHistory(DateTime DefaultDate, IEnumerable<DateTime> Days);