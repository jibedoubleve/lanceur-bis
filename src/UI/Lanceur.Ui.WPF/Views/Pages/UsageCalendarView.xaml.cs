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
            if (referenceDate is null) return;

            var days = referenceDate.GetDaysOfMonth();
            var history = viewModel.GetHistoryOfMonth(referenceDate).ToArray();
            foreach (var item in history)
            {
                days.Remove(item);
            }
            
            //Set the selected date to the first free day
            calendar.SelectedDate = history.FirstOrDefault();
            
            UsageCalendar.BlackoutDates.Clear();
            UsageCalendar.BlackoutDates.AddRange(days.Select(x=> new CalendarDateRange(x)));
        }
    }

    #endregion
}