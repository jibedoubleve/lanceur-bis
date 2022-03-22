using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;

namespace Lanceur.Views
{
    public class DoubloonsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IDataService _service;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public DoubloonsViewModel(
            IScheduler uiThread = null,
            IScheduler poolThread = null,
            IUserNotification notify = null,
            IDataService service = null,
            IThumbnailManager thumbnailManager = null)
        {
            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _service = service ?? l.GetService<IDataService>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();


            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveSelected = ReactiveCommand.Create(OnRemoveSelected, outputScheduler: uiThread);
            RemoveSelected.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .BindTo(this, vm => vm.Doubloons);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ObservableCollection<SelectableAliasQueryResult>> Activate { get; }
        [Reactive] public ObservableCollection<SelectableAliasQueryResult> Doubloons { get; set; }
        public ReactiveCommand<Unit, Unit> RemoveSelected { get; }

        #endregion Properties

        #region Methods

        private ObservableCollection<SelectableAliasQueryResult> OnActivate()
        {
            var doubloons = _service.GetDoubloons();
            _thumbnailManager.RefreshThumbnails(doubloons);
            var results = new ObservableCollection<SelectableAliasQueryResult>(doubloons.Cast<SelectableAliasQueryResult>());
            return results;
        }

        private void OnRemoveSelected()
        {
            var toDel = (from d in Doubloons
                         where d.IsSelected
                         select d).ToList();
            foreach (var item in toDel) { Doubloons.Remove(item); }
            _service.Remove(toDel);
            Toast.Information($"Removed {toDel.Count} alias(es).");
        }

        #endregion Methods
    }
}