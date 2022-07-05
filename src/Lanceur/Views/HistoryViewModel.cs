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
    public class HistoryViewModel : RoutableViewModel
    {
        #region Fields

        private readonly ILogService _log;
        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public HistoryViewModel(
            IScheduler uiThread = null,
            IDataService service = null,
            ILogService log = null,
            IUserNotification notify = null)
        {
            var l = Locator.Current;
            _service = service ?? l.GetService<IDataService>();
            _log = log ?? l.GetService<ILogService>();
            notify ??= l.GetService<IUserNotification>();

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, Unit> Activate { get; }

        public Action<IEnumerable<double>, IEnumerable<double>> OnRefreshChart { get; internal set; }

        #endregion Properties

        #region Methods

        private void OnActivate()
        {
            var history = _service.GetUsage(Per.Day);
            _log.Trace($"Loaded {history.Count()} item(s) from history");

            var x = history.Select(x => x.X.ToOADate());
            var y = history.Select(x => x.Y);

            OnRefreshChart(x, y);
        }

        #endregion Methods
    }
}