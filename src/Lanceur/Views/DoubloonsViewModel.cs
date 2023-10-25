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
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class DoubloonsViewModel : RoutableViewModel
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
            IUserNotification notify = null,
            IDbRepository service = null,
            IThumbnailManager thumbnailManager = null,
            INotification notification = null)
        {
            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            schedulers ??= l.GetService<ISchedulerProvider>();
            _service = service ?? l.GetService<IDbRepository>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
            _notification = notification ?? l.GetService<INotification>();
            _confirmRemove = Interactions.YesNoQuestion(schedulers.MainThreadScheduler);

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveSelected = ReactiveCommand.CreateFromTask(OnRemoveSelected, outputScheduler: schedulers.MainThreadScheduler);
            RemoveSelected.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .BindTo(this, vm => vm.Doubloons);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ObservableCollection<SelectableAliasQueryResult>> Activate { get; }
        public Interaction<string, bool> ConfirmRemove => _confirmRemove;
        [Reactive] public ObservableCollection<SelectableAliasQueryResult> Doubloons { get; set; }
        public ReactiveCommand<Unit, Unit> RemoveSelected { get; }

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
            var toDel = (from d in Doubloons
                         where d.IsSelected
                         select d).ToList();

            var count = toDel.Count;
            var remove = await _confirmRemove.Handle($"{count}");
            if (remove && count > 0)
            {
                foreach (var item in toDel) { Doubloons.Remove(item); }
                _service.Remove(toDel);
                _notification.Information($"Removed {toDel.Count} alias(es).");
            }
        }

        #endregion Methods
    }
}