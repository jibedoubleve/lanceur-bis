using DynamicData;
using DynamicData.Binding;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class KeywordsViewModel : RoutableViewModel, IValidatableViewModel
    {
        #region Fields

        private readonly SourceList<QueryResult> _aliases = new();
        private readonly IDataService _aliasService;
        private readonly Scope<bool> _busyScope;
        private readonly Interaction<string, bool> _confirmRemove;
        private readonly ILogService _log;
        private readonly IPackagedAppValidator _packagedAppValidator;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public KeywordsViewModel(
            ILogService logService = null,
            IDataService searchService = null,
            IScheduler uiThread = null,
            IScheduler poolThread = null,
            IUserNotification notify = null,
            IThumbnailManager thumbnailManager = null,
            IPackagedAppValidator packagedAppValidator = null)
        {
            _busyScope = new Scope<bool>(b => IsBusy = b, true, false);

            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _packagedAppValidator = packagedAppValidator ?? l.GetService<IPackagedAppValidator>();
            _log = logService ?? l.GetService<ILogService>(); ;
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
            _aliasService = searchService ?? l.GetService<IDataService>();
            _confirmRemove = Interactions.YesNoQuestion(uiThread);

            /*
             * VALIDATIONS
             */
            var canSaveOrUpdateAlias = this.WhenAnyValue(
                x => x.SelectedAlias.FileName,
                x => !string.IsNullOrEmpty(x)
            );
            this.ValidationRule(
                  vm => vm.SelectedAlias.FileName,
                  canSaveOrUpdateAlias,
                  "The path to the file shouldn't be empty."
              );

            /*
             * COMMANDS
             */

            Search = ReactiveCommand.Create<string, IEnumerable<QueryResult>>(OnSearch, outputScheduler: uiThread);
            Search.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            DuplicateAlias = ReactiveCommand.Create<Unit, AliasQueryResult>(OnDuplicateAlias, outputScheduler: uiThread);
            DuplicateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            CreateAlias = ReactiveCommand.Create(OnCreateAliasAsync, outputScheduler: uiThread);
            CreateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveAlias = ReactiveCommand.CreateFromTask<AliasQueryResult, Unit>(OnRemoveAliasAsync, outputScheduler: uiThread);
            RemoveAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveOrUpdateAlias = ReactiveCommand.CreateFromTask<AliasQueryResult, Unit>(OnSaveOrUpdateAlias, canSaveOrUpdateAlias, outputScheduler: uiThread);
            SaveOrUpdateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            /*
             * BINDINGS
             */
            _aliases
                .Connect()
                .ObserveOn(uiThread)
                .Bind(Aliases)
                .Subscribe();

            this.WhenAnyObservable(vm => vm.Search)
                .DistinctUntilChanged()
                .Log(this, "Setting aliases", c => $"Count {c.Count()}")
                .Subscribe(SetAliases);

            this.WhenAnyObservable(vm => vm.Search)
                .Where(x => x.Any())
                .Select(x => x.ElementAt(0) as AliasQueryResult)
                .Where(x => x is not null)
                .Log(this, "Selecting an alias", c => $"Id: {c.Id} - Name: {c.Name}")
                .BindTo(this, vm => vm.SelectedAlias);

            this.WhenAnyObservable(vm => vm.DuplicateAlias)
                .Select(x =>
                {
                    _log.Trace($"Duplicated alias '{x.Name}'");
                    _aliases.Add(x);
                    return x;
                })
                .BindTo(this, vm => vm.SelectedAlias);

            this.WhenAnyValue(vm => vm.SearchQuery)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Select(x => x?.Trim())
                .Log(this, $"Invoking search.", c => $"With criterion '{c}'")
                .InvokeCommand(Search);
        }

        #endregion Constructors

        #region Properties

        [Reactive] private bool IsSearchActivated { get; set; } = true;
        public IObservableCollection<QueryResult> Aliases { get; } = new ObservableCollectionExtended<QueryResult>();
        [Reactive] public string BusyMessage { get; set; }
        public Interaction<string, bool> ConfirmRemove => _confirmRemove;
        public ReactiveCommand<Unit, Unit> CreateAlias { get; }
        public ReactiveCommand<Unit, AliasQueryResult> DuplicateAlias { get; }

        [Reactive] public bool IsBusy { get; set; }

        public ReactiveCommand<AliasQueryResult, Unit> RemoveAlias { get; }

        public ReactiveCommand<AliasQueryResult, Unit> SaveOrUpdateAlias { get; }

        public ReactiveCommand<string, IEnumerable<QueryResult>> Search { get; }

        [Reactive] public string SearchQuery { get; set; }

        [Reactive] public AliasQueryResult SelectedAlias { get; set; }

        public ValidationContext ValidationContext { get; } = new ValidationContext();

        #endregion Properties

        #region Methods

        private void OnCreateAliasAsync()
        {
            var newAlias = AliasQueryResult.EmptyForCreation;
            _aliases.Insert(0, newAlias);
            SelectedAlias = newAlias;
        }

        private AliasQueryResult OnDuplicateAlias(Unit _)
        {
            var duplicated = SelectedAlias?.Duplicate();
            _aliasService.SaveOrUpdate(ref duplicated);
            return duplicated;
        }

        private async Task<Unit> OnRemoveAliasAsync(AliasQueryResult alias)
        {
            if (alias != null)
            {
                var remove = await _confirmRemove.Handle(alias.Name);
                if (remove)
                {
                    _log.Info($"User removed alias '{alias.Name}'");
                    _aliasService.Remove(alias);

                    if (_aliases.Remove(alias)) { Toast.Information($"Removed alias '{alias.Name}'."); }
                    else
                    {
                        var msg = $"Impossible to remove the alias '{alias.Name}'";
                        Toast.Warning(msg);
                        _log.Warning(msg);
                    }
                }
                else { _log.Debug($"User cancelled the remove of '{alias.Name}'."); }
            }
            return Unit.Default;
        }

        private async Task<Unit> OnSaveOrUpdateAlias(AliasQueryResult alias)
        {
            var created = alias.Id == 0;
            BusyMessage = "Saving alias...";
            using (_busyScope.Open())
            {
                alias = await _packagedAppValidator.FixAsync(alias);
                _aliasService.SaveOrUpdate(ref alias);
            }
            Toast.Information($"Alias '{alias.Name}' {(created ? "created" : "updated")}.");
            return Unit.Default;
        }

        private IEnumerable<QueryResult> OnSearch(string criterion)
        {
            var results = criterion.IsNullOrEmpty()
                ? _aliasService.GetAll()
                : _aliasService.Search(criterion);
            _thumbnailManager.RefreshThumbnails(results);
            return results;
        }

        private void SetAliases(IEnumerable<QueryResult> x)
        {
            _aliases.Clear();
            _aliases.AddRange(x);
        }

        public async Task Clear()
        {
            var scope = new Scope<bool>(t => IsSearchActivated = t, false, true);
            using (scope.Open())
            {
                SearchQuery = null;
                SelectedAlias = null;
                Aliases.Clear();
                await Task.Delay(50);
            }
        }

        #endregion Methods
    }
}