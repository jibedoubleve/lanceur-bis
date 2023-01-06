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
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class InvalidAliasViewModel : RoutableViewModel
    {
        #region Fields

        private readonly Interaction<string, bool> _confirmRemove;
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
            _confirmRemove = Interactions.YesNoQuestion(uiThread);

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveSelected = ReactiveCommand.CreateFromTask(OnRemoveSelected, outputScheduler: uiThread);
            RemoveSelected.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .BindTo(this, vm => vm.InvalidAliases);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ObservableCollection<SelectableAliasQueryResult>> Activate { get; }

        public Interaction<string, bool> ConfirmRemove => _confirmRemove;
        [Reactive] public ObservableCollection<SelectableAliasQueryResult> InvalidAliases { get; set; }

        public ReactiveCommand<Unit, Unit> RemoveSelected { get; }

        #endregion Properties

        #region Methods

        private ObservableCollection<SelectableAliasQueryResult> OnActivate()
        {
            var results = _service.GetInvalidAliases();
            return new(results);
        }

        private async Task OnRemoveSelected()
        {
            var toDel = (from d in InvalidAliases
                         where d.IsSelected
                         select d).ToList();
            var count = toDel?.Count() ?? 0;
            var remove = await _confirmRemove.Handle($"{count}");
            if (remove && count > 0)
            {
                foreach (var item in toDel) { InvalidAliases.Remove(item); }
                _service.Remove(toDel);
                Toast.Information($"Removed {toDel.Count} alias(es).");
            }
        }

        #endregion Methods
    }
}