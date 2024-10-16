using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views;

public class DoubloonsViewModel : RoutableViewModel, IActivatableViewModel
{
    #region Fields

    private readonly Interaction<string, bool> _confirmRemove;
    private readonly INotification _notification;
    private readonly IDbRepository _service;
    private readonly IThumbnailManager _thumbnailManager;

    #endregion Fields

    #region Constructors

    public DoubloonsViewModel(
        ISchedulerProvider schedulers = null,
        IUiNotification notify = null,
        IDbRepository service = null,
        IThumbnailManager thumbnailManager = null,
        INotification notification = null
    )
    {
        var l = Locator.Current;
        notify ??= l.GetService<IUiNotification>();
        schedulers ??= l.GetService<ISchedulerProvider>();
        _service = service ?? l.GetService<IDbRepository>();
        _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
        _notification = notification ?? l.GetService<INotification>();
        _confirmRemove = Interactions.YesNoQuestion(schedulers.MainThreadScheduler);

        this.WhenActivated(
            d =>
            {
                Activate = ReactiveCommand.Create(OnActivate, outputScheduler: schedulers.TaskpoolScheduler);
                Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
                Activate.DisposeWith(d);

                RemoveSelected =
                    ReactiveCommand.CreateFromTask(OnRemoveSelected, outputScheduler: schedulers.MainThreadScheduler);
                RemoveSelected.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
                RemoveSelected.DisposeWith(d);

                this.WhenAnyObservable(vm => vm.Activate)
                    .BindTo(this, vm => vm.Doubloons)
                    .DisposeWith(d);

                this.WhenAnyValue(vm => vm.IsActivating)
                    .Where(x => x)
                    .Select(_ => Unit.Default)
                    .InvokeCommand(Activate)
                    .DisposeWith(d);

                Activate.WhenAnyObservable(x => x.IsExecuting)
                        .Where(x => !x)
                        .BindTo(this, vm => vm.IsActivating)
                        .DisposeWith(d);

                IsActivating = true;
                // Activate.Execute().Subscribe().DisposeWith(d);
            }
        );
    }

    #endregion Constructors

    #region Properties

    [Reactive] private bool IsActivating { get; set; }
    public ReactiveCommand<Unit, ObservableCollection<SelectableAliasQueryResult>> Activate { get; private set; }

    public ViewModelActivator Activator { get; } = new();
    public Interaction<string, bool> ConfirmRemove => _confirmRemove;
    [Reactive] public ObservableCollection<SelectableAliasQueryResult> Doubloons { get; set; }
    public ReactiveCommand<Unit, Unit> RemoveSelected { get; private set; }

    #endregion Properties

    #region Methods

    private ObservableCollection<SelectableAliasQueryResult> OnActivate()
    {
        var doubloons = _service.GetDoubloons().ToArray();
        _thumbnailManager.RefreshThumbnails(doubloons);
        var results = new ObservableCollection<SelectableAliasQueryResult>(doubloons.Cast<SelectableAliasQueryResult>());
        return results;
    }

    private async Task OnRemoveSelected()
    {
        var toDel = Doubloons.Where(d => d.IsSelected)
                             .ToList();

        var count = toDel.Count;
        var remove = await _confirmRemove.Handle($"{count}");
        if (remove && count > 0)
        {
            foreach (var item in toDel) Doubloons.Remove(item);
            _service.RemoveMany(toDel);
            _notification.Information($"Removed {toDel.Count} alias(es).");
        }
    }

    #endregion Methods
}