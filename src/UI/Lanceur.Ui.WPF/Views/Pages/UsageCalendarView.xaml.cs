using System.Windows.Controls;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class UsageCalendarView
{
    #region Constructors

    public UsageCalendarView(UsageCalendarViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        UsageCalendar.DisplayDateChanged += (sender, args) => RefreshBlackouts(args.AddedDate, (Calendar)sender!);
        UsageCalendar.DisplayDate = DateTime.Today;
        UsageCalendar.SelectedDate = DateTime.Today;
        Loaded += (sender, _) => RefreshBlackouts(DateTime.Today, ((UsageCalendarView)sender).UsageCalendar);
        return;

        void RefreshBlackouts(DateTime? referenceDate, Calendar calendar)
        {
            if (referenceDate is null) { return; }

            var days = referenceDate.GetDaysOfMonth();
            var history = viewModel.GetHistoryOfMonth(referenceDate).ToArray();
            foreach (var item in history) days.Remove(item);

            calendar.SelectedDate = SelectDate(history);

            UsageCalendar.BlackoutDates.Clear();
            UsageCalendar.BlackoutDates.AddRange(days.Select(x => new CalendarDateRange(x)));
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Selects the date to display from a month's alias-usage history.
    ///     Returns today's date if it is the most recent entry in <paramref name="history" />;
    ///     otherwise returns the earliest recorded date in the month.
    /// </summary>
    private static DateTime? SelectDate(DateTime[] history)
    {
        var max = history.Max();
        return max.Date == DateTime.Today
            ? max
            : history.FirstOrDefault();
    }

    #endregion
}