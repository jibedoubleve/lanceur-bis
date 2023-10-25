using DynamicData;
using DynamicData.Binding;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Lanceur.Schedulers;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class KeywordsViewModel : RoutableViewModel, IValidatableViewModel, IActivatableViewModel
    {
        #region Fields

        private readonly SourceList<QueryResult> _aliases = new();
        private readonly IDbRepository _aliasService;
        private readonly Scope<bool> _busyScope;
        private readonly Interaction<string, bool> _confirmRemove;
        private readonly IAppLogger _log;
        private readonly INotification _notification;
        private readonly IPackagedAppValidator _packagedAppValidator;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public KeywordsViewModel(
            IAppLoggerFactory logFactory = null,
            IDbRepository searchService = null,
            ISchedulerProvider schedulers = null,
            IUserNotification notify = null,
            IThumbnailManager thumbnailManager = null,
            IPackagedAppValidator packagedAppValidator = null,
            INotification notification = null)
        {
            _busyScope = new(b => IsBusy = b, true, false);
            schedulers ??= Locator.Current.GetService<ISchedulerProvider>();

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _packagedAppValidator = packagedAppValidator ?? l.GetService<IPackagedAppValidator>();
            _notification = notification ?? l.GetService<INotification>();
            _log = l.GetLogger<KeywordsViewModel>(logFactory);
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
            _aliasService = searchService ?? l.GetService<IDbRepository>();
            _confirmRemove = Interactions.YesNoQuestion(schedulers.MainThreadScheduler);

            this.WhenActivated(d =>
            {
                Disposable
                .Create(() =>
                {
                    Aliases.Clear();
                    AliasToCreate = null;
                }).DisposeWith(d);

                SetupValidations(d);
                SetupCommands(schedulers.MainThreadScheduler, notify, d);
                SetupBindings(schedulers.MainThreadScheduler, d);
            });
        }

        #endregion Constructors

        #region Properties

        public ViewModelActivator Activator { get; } = new();

        public IObservableCollection<QueryResult> Aliases { get; } = new ObservableCollectionExtended<QueryResult>();

        [Reactive] public AliasQueryResult AliasToCreate { get; set; }

        [Reactive] public string BusyMessage { get; set; }

        public Interaction<string, bool> ConfirmRemove => _confirmRemove;

        public ReactiveCommand<Unit, Unit> CreateAlias { get; private set; }

        [Reactive] public bool IsBusy { get; set; }

        public ReactiveCommand<AliasQueryResult, Unit> RemoveAlias { get; private set; }

        public ReactiveCommand<AliasQueryResult, SaveOrUpdateAliasResponse> SaveOrUpdateAlias { get; private set; }

        private ReactiveCommand<SearchRequest, IEnumerable<QueryResult>> Search { get;  set; }

        [Reactive] public string SearchQuery { get; set; }

        [Reactive] public AliasQueryResult SelectedAlias { get; set; }

        public ValidationHelper ValidationAliasExists { get; private set; }

        public ValidationContext ValidationContext { get; } = new ValidationContext();

        public ValidationHelper ValidationFileName { get; private set; }

        #endregion Properties

        #region Methods

        private void OnCreateAlias()
        {
            var newAlias = AliasQueryResult.EmptyForCreation;
            _aliases.Insert(0, newAlias);
            SelectedAlias = newAlias;
        }

        private async Task<Unit> OnRemoveAliasAsync(AliasQueryResult alias)
        {
            if (alias == null) return Unit.Default;

            var remove = await _confirmRemove.Handle(alias.Name);
            if (remove)
            {
                _log.Info($"User removed alias '{alias.Name}'");
                _aliasService.Remove(alias);

                if (_aliases.Remove(alias)) { _notification.Information($"Removed alias '{alias.Name}'."); }
                else
                {
                    var msg = $"Impossible to remove the alias '{alias.Name}'";
                    _notification.Warning(msg);
                    _log.Warning(msg);
                }
            }
            else { _log.Debug($"User cancelled the remove of '{alias.Name}'."); }
            return Unit.Default;
        }

        private async Task<SaveOrUpdateAliasResponse> OnSaveOrUpdateAliasAsync(AliasQueryResult alias)
        {
            var created = alias.Id == 0;
            BusyMessage = "Saving alias...";
            using (_busyScope.Open())
            {
                alias = await _packagedAppValidator.FixAsync(alias);
                _aliasService.SaveOrUpdate(ref alias);
            }
            _notification.Information($"Alias '{alias.Name}' {(created ? "created" : "updated")}.");

            // Returns updated results
            return new()
            {
                Aliases = _aliasService.GetAll()
            };
        }

        private IEnumerable<QueryResult> OnSearch(SearchRequest request)
        {
            var results = request.Query.IsNullOrEmpty()
                ? _aliasService.GetAll().ToList()
                : _aliasService.Search(request.Query).ToList();
            _thumbnailManager.RefreshThumbnails(results);

            if (request.AliasToCreate == null) return results;

            results.Insert(0, request.AliasToCreate);
            return results;

        }

        private void SetAliases(IEnumerable<QueryResult> x)
        {
            _aliases.Clear();
            _aliases.AddRange(x);
        }

        private void SetupBindings(IScheduler uiThread, CompositeDisposable d)
        {
            _aliases
                .Connect()
                .ObserveOn(uiThread)
                .Bind(Aliases)
                .Subscribe()
                .DisposeWith(d);

            this.WhenAnyObservable(vm => vm.Search)
                .DistinctUntilChanged()
                .Log(this, "Setting aliases", c => $"Count {c.Count()}")
                .Subscribe(SetAliases)
                .DisposeWith(d);

            this.WhenAnyObservable(vm => vm.Search)
                .Where(x => x.Any())
                .Select(x => x.ElementAt(0) as AliasQueryResult)
                .Where(x => x is not null)
                .Log(this, "Selecting an alias", c => $"Id: {c.Id} - Name: {c.Name}")
                .BindTo(this, vm => vm.SelectedAlias)
                .DisposeWith(d);

            this.WhenAnyValue(vm => vm.SearchQuery)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Select(x => new SearchRequest(x?.Trim(), AliasToCreate))
                .Log(this, $"Invoking search.", c => $"With criterion '{c.Query}' and alias to create '{(c.AliasToCreate?.Name ?? "<EMPTY>")}'")
                .InvokeCommand(Search)
                .DisposeWith(d);

            this.WhenAnyObservable(vm => vm.SaveOrUpdateAlias)
                .Select(x => x.Aliases.ToObservableCollection())
                .Subscribe(SetAliases);
        }

        private void SetupCommands(IScheduler uiThread, IUserNotification notify, CompositeDisposable d)
        {
            Search = ReactiveCommand.Create<SearchRequest, IEnumerable<QueryResult>>(OnSearch, outputScheduler: uiThread).DisposeWith(d);
            Search.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            CreateAlias = ReactiveCommand.Create(OnCreateAlias, outputScheduler: uiThread).DisposeWith(d);
            CreateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveAlias = ReactiveCommand.CreateFromTask<AliasQueryResult, Unit>(OnRemoveAliasAsync, outputScheduler: uiThread).DisposeWith(d);
            RemoveAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveOrUpdateAlias = ReactiveCommand.CreateFromTask<AliasQueryResult, SaveOrUpdateAliasResponse>(OnSaveOrUpdateAliasAsync, this.IsValid(), outputScheduler: uiThread).DisposeWith(d);
            SaveOrUpdateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
        }

        private void SetupValidations(CompositeDisposable d)
        {
            var fileNameExists = this.WhenAnyValue(
                x => x.SelectedAlias.FileName,
                x => !x.IsNullOrEmpty()
            );
            var nameExists = this.WhenAnyValue(
                x => x.SelectedAlias.SynonymsNextState,
                x => _aliasService.CheckNamesExist(x.SplitCsv())
            );

            ValidationFileName = this.ValidationRule(
                   vm => vm.SelectedAlias.FileName,
                   fileNameExists,
                   "The path to the file shouldn't be empty."
             );
            ValidationFileName.DisposeWith(d);

            ValidationAliasExists = this.ValidationRule(
                   vm => vm.SelectedAlias.Synonyms,
                   nameExists,
                   response => !response.Exists,
                   response => (response.Exists == false && !response.ExistingNames.Any())
                       ? "The names should not be empty"
                       : $"'{response.ExistingNames.JoinCsv()}' {(response.ExistingNames.Length <= 1 ? "is" : "are")} already used as alias.");
            ValidationAliasExists.DisposeWith(d);
        }

        public async Task Clear()
        {
            SearchQuery = null;
            SelectedAlias = null;
            Aliases.Clear();
            await Task.Delay(50);
        }

        #endregion Methods

        #region Classes

        public class SaveOrUpdateAliasResponse
        {
            #region Properties

            public IEnumerable<QueryResult> Aliases { get; init; }

            #endregion Properties
        }

        private class SearchRequest
        {
            #region Constructors

            public SearchRequest(string query, AliasQueryResult alias = null)
            {
                Query = query;
                AliasToCreate = alias;
            }

            #endregion Constructors

            #region Properties

            public AliasQueryResult AliasToCreate { get; }
            public string Query { get; }

            #endregion Properties
        }

        #endregion Classes

        public void HydrateSelectedAlias() => _aliasService.HydrateAlias(SelectedAlias);
    }
}