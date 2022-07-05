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
    public class InvalidAliasViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public InvalidAliasViewModel(
                    IScheduler uiThread = null,
            IScheduler poolThread = null,
            IUserNotification notify = null,
            IDataService service = null)
        {
            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _service = service ?? l.GetService<IDataService>();

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveSelected = ReactiveCommand.Create(OnRemoveSelected, outputScheduler: uiThread);
            RemoveSelected.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .BindTo(this, vm => vm.InvalidAliases);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ObservableCollection<SelectableAliasQueryResult>> Activate { get; }

        [Reactive] public ObservableCollection<SelectableAliasQueryResult> InvalidAliases { get; set; }

        public ReactiveCommand<Unit, Unit> RemoveSelected { get; }

        #endregion Properties

        #region Methods

        private ObservableCollection<SelectableAliasQueryResult> OnActivate()
        {
            var results = _service.GetInvalidAliases();
            return new(results);
        }

        private void OnRemoveSelected()
        {
            var toDel = (from d in InvalidAliases
                         where d.IsSelected
                         select d).ToList();
            foreach (var item in toDel) { InvalidAliases.Remove(item); }
            _service.Remove(toDel);
            Toast.Information($"Removed {toDel.Count} alias(es).");
        }

        #endregion Methods
    }
}