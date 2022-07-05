using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace Lanceur.Views
{
    public class TrendsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public TrendsViewModel(
            IScheduler uiThread = null,
            IDataService service = null,
            IUserNotification notify = null)
        {
            var l = Locator.Current;
            _service = service ?? l.GetService<IDataService>();
            notify ??= l.GetService<IUserNotification>();

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, Unit> Activate { get; }
        public Action<IEnumerable<string>, IEnumerable<double>> OnRefreshChartDayOfWeek { get; internal set; }
        public Action<IEnumerable<string>, IEnumerable<double>> OnRefreshChartHour { get; internal set; }
        public Action<IEnumerable<string>, IEnumerable<double>> OnRefreshChartMonth { get; internal set; }

        #endregion Properties

        #region Methods

        private void OnActivate()
        {
            var dow = _service.GetUsage(Per.DayOfWeek);
            var x2 = dow.Select(x => x.X.ToString("dddd"));
            var y2 = dow.Select(x => x.Y);
            OnRefreshChartDayOfWeek?.Invoke(x2, y2);

            var hour = _service.GetUsage(Per.Hour);
            var x3 = hour.Select(x => x.X.ToString("HH:mm"));
            var y3 = hour.Select(x => x.Y);
            OnRefreshChartHour?.Invoke(x3, y3);

            var month = _service.GetUsage(Per.Month);
            var x4 = month.Select(x => x.X.ToString("MMM yy"));
            var y4 = month.Select(x => x.Y);
            OnRefreshChartMonth?.Invoke(x4, y4);
        }

        #endregion Methods

        #region Classes

        private class ActivationContext
        {
            #region Properties

            public IEnumerable<DataPoint<DateTime, double>> Day { get; set; }
            public IEnumerable<DataPoint<DateTime, double>> Hour { get; set; }
            public IEnumerable<DataPoint<DateTime, double>> Month { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}