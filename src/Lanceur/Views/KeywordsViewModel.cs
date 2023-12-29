using DynamicData;
using DynamicData.Binding;
using Lanceur.Core.LuaScripting;
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
        private readonly IAppLogger _log;
        private readonly INotification _notification;
        private readonly IUserNotification _notify;
        private readonly IPackagedAppSearchService _packagedAppSearchService;
        private readonly ISchedulerProvider _schedulers;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public KeywordsViewModel(
            IAppLoggerFactory logFactory = null,
            IDbRepository searchService = null,
            ISchedulerProvider schedulers = null,
            IUserNotification notify = null,
            IThumbnailManager thumbnailManager = null,
            INotification notification = null,
            IPackagedAppSearchService packagedAppSearchService = null)
        {
            _busyScope = new(b => IsBusy = b, true, false);
            _schedulers = schedulers ?? Locator.Current.GetService<ISchedulerProvider>();

            var l = Locator.Current;
            _notify = notify ?? l.GetService<IUserNotification>();
            _packagedAppSearchService = packagedAppSearchService ?? l.GetService<IPackagedAppSearchService>();
            _notification = notification ?? l.GetService<INotification>();
            _log = l.GetLogger<KeywordsViewModel>(logFactory);
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
            _aliasService = searchService ?? l.GetService<IDbRepository>();

            ConfirmRemove = Interactions.YesNoQuestion(_schedulers.MainThreadScheduler);
            AskLuaEditor = new();

            this.WhenActivated(Activate);
        }

        #endregion Constructors

        #region Properties

        private ReactiveCommand<SearchRequest, IEnumerable<QueryResult>> Search { get; set; }

        public ViewModelActivator Activator { get; } = new();

        public IObservableCollection<QueryResult> Aliases { get; } = new ObservableCollectionExtended<QueryResult>();

        [Reactive] public AliasQueryResult AliasToCreate { get; set; }

        public Interaction<Script, string> AskLuaEditor { get; }

        [Reactive] public string BusyMessage { get; set; }

        public Interaction<string, bool> ConfirmRemove { get; }

        public ReactiveCommand<Unit, Unit> CreatingAlias { get; private set; }

        public ReactiveCommand<Unit, string> EditLuaScript { get; private set; }

        [Reactive] public bool IsBusy { get; set; }

        public ReactiveCommand<AliasQueryResult, Unit> RemoveAlias { get; private set; }

        public ReactiveCommand<AliasQueryResult, AliasQueryResult> SaveOrUpdateAlias { get; private set; }

        [Reactive] public string SearchQuery { get; set; }

        [Reactive] public AliasQueryResult SelectedAlias { get; set; }

        public ValidationHelper ValidationAliasExists { get; private set; }

        public ValidationContext ValidationContext { get; } = new ValidationContext();

        public ValidationHelper ValidationFileName { get; private set; }

        #endregion Properties

        #region Methods

        private void OnCreatingAlias()
        {
            if (Aliases.Any(x => x.Id == 0)) return;

            var newAlias = AliasQueryResult.EmptyForCreation;
            _aliases.Insert(0, newAlias);
            SelectedAlias = newAlias;
        }

        private async Task<string> OnEditLuaScript()
        {
            var output = await AskLuaEditor.Handle(SelectedAlias.ToScript());
            SelectedAlias.LuaScript = output;
            return output;
        }

        private async Task<Unit> OnRemoveAliasAsync(AliasQueryResult alias)
        {
            if (alias == null) return Unit.Default;

            var remove = await ConfirmRemove.Handle(alias.Name);
            if (remove)
            {
                _log.Info($"User removed alias '{alias.Name}'");
                _aliasService.Remove(alias);

                if (_aliases.Remove(alias)) { _notification.Information($"Removed alias '{alias.Name}'."); }
                else { _log.Warning($"Impossible to remove the alias '{alias.Name}'"); }
            }
            else { _log.Debug($"User cancelled the remove of '{alias.Name}'."); }
            return Unit.Default;
        }

        private AliasQueryResult OnSaveOrUpdateAlias(AliasQueryResult alias)
        {
            var created = alias.Id == 0;
            BusyMessage = "Saving alias...";
            using (_busyScope.Open())
            {
                try
                {
                    alias.SetName();
                    var response = _packagedAppSearchService.GetByInstalledDirectory(alias.FileName)
                                                            .FirstOrDefault();
                    if (response is not null) // This is a packaged application
                    {
                        alias.FileName = $"package:{response.AppUserModelId}";
                        alias.Description = response.DisplayName.IsNullOrWhiteSpace() ? "Packaged app" : response.DisplayName;
                    }

                    _aliasService.SaveOrUpdate(ref alias);
                }
                catch (ApplicationException ex)
                {
                    _log.Warning(ex.Message);
                    _notify.Warning($"Error when {(created ? "creating" : "updating")} the alias: {ex.Message}");
                    return alias;
                }
            }
            _notification.Information($"Alias '{alias.Name}' {(created ? "created" : "updated")}.");

            return alias;
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
            var selected = Aliases.FirstOrDefault(a => a.Id == SelectedAlias?.Id);
            _aliases.Clear();
            _aliases.AddRange(x);

            if (selected is AliasQueryResult alias)
            {
                SelectedAlias = alias;
            }
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

            this.WhenAnyObservable(vm => vm.SaveOrUpdateAlias)
                .Where(alias => alias is not null)
                .Log(this, "Saved or updated alias.", c => $"Id: {c.Id} - Name: {c.Name}")
                .Subscribe(UpdateAliasWithSynonyms)
                .DisposeWith(d);

            this.WhenAnyValue(vm => vm.SearchQuery)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Select(x => new SearchRequest(x?.Trim(), AliasToCreate))
                .Log(this, "Invoking search.", c => $"With criterion '{c.Query}' and alias to create '{c.AliasToCreate?.Name ?? "<EMPTY>"}'")
                .InvokeCommand(Search)
                .DisposeWith(d);
        }

        private void SetupCommands(IScheduler uiThread, IUserNotification notify, CompositeDisposable d)
        {
            Search = ReactiveCommand.Create<SearchRequest, IEnumerable<QueryResult>>(OnSearch, outputScheduler: uiThread).DisposeWith(d);
            Search.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            CreatingAlias = ReactiveCommand.Create(OnCreatingAlias, outputScheduler: uiThread).DisposeWith(d);
            CreatingAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveAlias = ReactiveCommand.CreateFromTask<AliasQueryResult, Unit>(OnRemoveAliasAsync, outputScheduler: uiThread).DisposeWith(d);
            RemoveAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveOrUpdateAlias = ReactiveCommand.Create<AliasQueryResult, AliasQueryResult>(
                OnSaveOrUpdateAlias,
                this.IsValid(),
                outputScheduler: uiThread
            ).DisposeWith(d);
            SaveOrUpdateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            EditLuaScript = ReactiveCommand.CreateFromTask(OnEditLuaScript, outputScheduler: uiThread).DisposeWith(d);
            EditLuaScript.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
        }

        private void SetupValidations(CompositeDisposable d)
        {
            var validateFileNameExists = this.WhenAnyValue(
                x => x.SelectedAlias.FileName,
                x => !x.IsNullOrEmpty()
            );
            var validateNameExists = this.WhenAnyValue(
                x => x.SelectedAlias.SynonymsToAdd,
                x => _aliasService.SelectNames(x.SplitCsv())
            );

            ValidationFileName = this.ValidationRule(
                   vm => vm.SelectedAlias.FileName,
                   validateFileNameExists,
                   "The path to the file shouldn't be empty."
             );
            ValidationFileName.DisposeWith(d);

            ValidationAliasExists = this.ValidationRule(
                   vm => vm.SelectedAlias.Synonyms,
                   validateNameExists,
                   response => !response?.Exists ?? false,
                   response =>
                   {
                       if (response == null) return "The names should not be empty.";
                       
                       return !response.Exists && !response.ExistingNames.Any()
                           ? "The names should not be empty."
                           : $"'{response.ExistingNames.JoinCsv()}' {(response.ExistingNames.Length <= 1 ? "is" : "are")} already used as alias.";
                   });
            ValidationAliasExists.DisposeWith(d);
        }

        private void UpdateAliasWithSynonyms(AliasQueryResult alias)
        {
            var toDel = _aliases.Items
                                .Where(a => a.Id == alias.Id)
                                .ToArray();
            foreach (var item in toDel)
            {
                _aliases.Remove(item);
            }

            var names = alias.Synonyms.SplitCsv();
            foreach (var name in names)
            {
                var toAdd = alias.CloneObject();
                toAdd.Name = name;
                _aliases.Add(toAdd);
            }
        }

        /// <summary>
        /// The purpose of this method is for unit test only. You can manually mimic
        /// <code>WhenActivated</code>
        /// </summary>
        /// <param name="d">
        /// Ensures the provided disposable is disposed with the specified <see cref="CompositeDisposable"/>.
        /// </param>
        internal void Activate(CompositeDisposable d)
        {
            Disposable
                .Create(() =>
                {
                    Aliases.Clear();
                    AliasToCreate = null;
                }).DisposeWith(d);

            SetupValidations(d);
            SetupCommands(_schedulers.MainThreadScheduler, _notify, d);
            SetupBindings(_schedulers.MainThreadScheduler, d);
        }

        public void HydrateSelectedAlias() => _aliasService.HydrateAlias(SelectedAlias);

        #endregion Methods

        #region Classes

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
    }
}