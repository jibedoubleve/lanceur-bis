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

            var canSearch = this.WhenAnyValue(x => x.SearchQuery).Select(x => !string.IsNullOrWhiteSpace(x));
            Search = ReactiveCommand.Create<string, IEnumerable<QueryResult>>(OnSearch, canSearch, outputScheduler: uiThread);
            Search.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            DuplicateAlias = ReactiveCommand.Create<Unit, AliasQueryResult>(OnDuplicateAlias, outputScheduler: uiThread);
            DuplicateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.CreateFromTask<string, IEnumerable<QueryResult>>(OnActivateAsync, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

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

            this.WhenAnyObservable(vm => vm.Search, vm => vm.Activate)
                .DistinctUntilChanged()
                .Subscribe(SetAliases);

            this.WhenAnyObservable(vm => vm.Search, vm => vm.Activate)
                .Where(x => x.Any())
                .Select(x => x.ElementAt(0) as AliasQueryResult)
                .Where(x => x is not null)
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
                .InvokeCommand(Search);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<string, IEnumerable<QueryResult>> Activate { get; }

        public IObservableCollection<QueryResult> Aliases { get; } = new ObservableCollectionExtended<QueryResult>();

        [Reactive] public string BusyMessage { get; set; }

        public Interaction<string, bool> ConfirmRemove => _confirmRemove;

        public ReactiveCommand<Unit, AliasQueryResult> DuplicateAlias { get; }

        public bool IsActivatable { get; internal set; }

        [Reactive] public bool IsBusy { get; set; }

        public ReactiveCommand<AliasQueryResult, Unit> RemoveAlias { get; }

        public ReactiveCommand<AliasQueryResult, Unit> SaveOrUpdateAlias { get; }

        public ReactiveCommand<string, IEnumerable<QueryResult>> Search { get; }

        [Reactive] public string SearchQuery { get; set; }

        [Reactive] public AliasQueryResult SelectedAlias { get; set; }

        public ValidationContext ValidationContext { get; } = new ValidationContext();

        #endregion Properties

        #region Methods

        private async Task<IEnumerable<QueryResult>> OnActivateAsync(string aliasName)
        {
            var results = await Task.Run(() => _aliasService.GetAll().ToList());

            if (!aliasName.IsNullOrEmpty())
            {
                var toAdd = aliasName.IsNullOrWhiteSpace()
                    ? AliasQueryResult.EmptyForCreation
                    : AliasQueryResult.FromName(aliasName);

                results.Insert(0, toAdd);
            }

            _thumbnailManager.RefreshThumbnails(results);
            return results;
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
                    _log.Trace($"User removed alias '{alias.Name}'");
                    _aliasService.Remove(alias);
                    _aliases.Remove(alias);
                    Toast.Information($"Removed alias '{alias.Name}'.");
                }
                else { _log.Trace($"User cancelled the remove of '{alias.Name}'."); }
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

            SelectedAlias = x?.FirstOrDefault() as AliasQueryResult;
        }

        #endregion Methods
    }
}