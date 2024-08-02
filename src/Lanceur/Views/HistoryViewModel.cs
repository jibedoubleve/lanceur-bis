using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Schedulers;
using Lanceur.Ui;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Lanceur.Views;

public class HistoryViewModel : RoutableViewModel
{
    #region Fields

    private readonly ILogger<HistoryViewModel> _logger;
    private readonly ISchedulerProvider _schedulers;
    private readonly IDbRepository _service;

    #endregion Fields

    #region Constructors

    public HistoryViewModel(
        ISchedulerProvider schedulers = null,
        IDbRepository service = null,
        ILoggerFactory logFactory = null,
        IUserNotification notify = null
    )
    {
        var l = Locator.Current;
        _schedulers = schedulers ?? l.GetService<ISchedulerProvider>();
        _service = service ?? l.GetService<IDbRepository>();
        _logger = logFactory.GetLogger<HistoryViewModel>();
        notify ??= l.GetService<IUserNotification>();

        Activate = ReactiveCommand.Create(OnActivate, outputScheduler: _schedulers.MainThreadScheduler);
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
        var history = _service.GetUsage(Per.Day).ToList();
        _logger.LogDebug("Loaded {Count} item(s) from history", history.Count);

        var x = history.Select(p => p.X.ToOADate());
        var y = history.Select(p => p.Y);

        OnRefreshChart(x, y);
    }

    #endregion Methods
}