using Lanceur.Core.LuaScripting;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Schedulers;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using DynamicData;
using Humanizer;
using Lanceur.Core.BusinessLogic;
using Lanceur.Utils;

namespace Lanceur.Views
{
    public class KeywordsViewModel : RoutableViewModel, IValidatableViewModel, IActivatableViewModel
    {
        #region Fields

        private readonly IDbRepository _aliasService;
        private readonly Scope<bool> _busyScope;
        private readonly ILogger<KeywordsViewModel> _logger;
        private readonly INotification _notification;
        private readonly IUserNotification _notify;
        private readonly IPackagedAppSearchService _packagedAppSearchService;
        private readonly ISchedulerProvider _schedulers;
        private readonly IThumbnailManager _thumbnailManager;

        #endregion Fields

        #region Constructors

        public KeywordsViewModel(
            ILoggerFactory logFactory = null,
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
            _logger = logFactory.GetLogger<KeywordsViewModel>();
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
            _aliasService = searchService ?? l.GetService<IDbRepository>();

            ConfirmRemove = Interactions.YesNoQuestion(_schedulers.MainThreadScheduler);
            AskLuaEditor = new();

            this.WhenActivated(Activate);
        }

        #endregion Constructors

        #region Properties

        private ReactiveCommand<SearchRequest, ObservableCollection<QueryResult>> Search { get; set; }

        public ViewModelActivator Activator { get; } = new();

        [Reactive] public ObservableCollection<QueryResult> Aliases { get; set; } = new(); 

        [Reactive] public AliasQueryResult AliasToCreate { get; set; }

        public Interaction<Script, string> AskLuaEditor { get; }

        [Reactive] public string BusyMessage { get; set; }

        public Interaction<string, bool> ConfirmRemove { get; }

        public ReactiveCommand<Unit, QueryResult> CreatingAlias { get; private set; }

        public ReactiveCommand<Unit, string> EditLuaScript { get; private set; }

        [Reactive] public bool IsBusy { get; set; }

        public ReactiveCommand<AliasQueryResult, AliasQueryResult> RemoveAlias { get; private set; }

        public ReactiveCommand<AliasQueryResult, QueryResult> SaveOrUpdateAlias { get; private set; }

        [Reactive] public string SearchQuery { get; set; }

        [Reactive] public AliasQueryResult SelectedAlias { get; set; }

        public ValidationHelper ValidationAliasExists { get; private set; }

        public ValidationContext ValidationContext { get; } = new();

        public ValidationHelper ValidationFileName { get; private set; }

        #endregion Properties

        #region Methods

        private QueryResult OnCreatingAlias()
        {
            return Aliases.Any(x => x.Id == 0) 
                ? null 
                : AliasQueryResult.EmptyForCreation;
        }

        private async Task<string> OnEditLuaScript()
        {
            var output = await AskLuaEditor.Handle(SelectedAlias.ToScript());
            SelectedAlias.LuaScript = output;
            return output;
        }

        private async Task<AliasQueryResult> OnRemoveAliasAsync(AliasQueryResult alias)
        {
            using var _ = _logger.BeginSingleScope("RemovedAlias", alias);
            if (alias == null) return null;

            var remove = await ConfirmRemove.Handle(alias.Name);
            if (!remove)
            {
                _logger.LogDebug("User cancelled the remove of {Name}", alias.Name);
                return alias;
            }

            _aliasService.Remove(alias);
            _logger.LogInformation("User removed alias {Name}", alias.Name);
            return alias;
        }

        private QueryResult OnSaveOrUpdateAlias(AliasQueryResult alias)
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
                    _logger.LogWarning(ex, "An error occured while saving or updating alias");
                    _notify.Warning($"Error when {(created ? "creating" : "updating")} the alias.");
                    return null;
                }
            }
            _notification.Information($"Alias '{alias.Name}' {(created ? "created" : "updated")}.");

            return alias;
        }

        private ObservableCollection<QueryResult> OnSearch(SearchRequest request)
        {
            var results = request.Query.IsNullOrEmpty()
                ? _aliasService.GetAll().ToList()
                : _aliasService.Search(request.Query).ToList();
            _thumbnailManager.RefreshThumbnailsAsync(results);

            results.SortCollection(a => a.Name);
            if (request.AliasToCreate == null) return new(results);

            results.Insert(0, request.AliasToCreate);
            return new(results);
        }

        private void SetupBindings(IScheduler uiThread, CompositeDisposable d)
        {
            // REMOVING ALIAS
            this.WhenAnyObservable(vm => vm.RemoveAlias)
                .Where(x => x is not null)
                .Subscribe(x => Aliases.Remove(x))
                .DisposeWith(d);
            
            // ADDING ALIAS
            this.WhenAnyObservable(vm => vm.CreatingAlias)
                .Select(x => x as AliasQueryResult)
                .Where(x => x is not null)
                .Subscribe(newAlias =>
                {
                    Aliases.Insert(0, newAlias);
                    SelectedAlias = newAlias;
                })
                .DisposeWith(d);
            
            // SEARCH ALIAS
            this.WhenAnyObservable(vm => vm.Search)
                .Select(x => x.ToObservableCollection())
                .Log(this, "Search executed", c => $"Found {c.Count} item(s)")
                .BindTo(this, vm => vm.Aliases)
                .DisposeWith(d);

            this.WhenAnyObservable(vm => vm.Search)
                .Where(x => x.Any())
                .Select(x => x.ElementAt(0) as AliasQueryResult)
                .Where(x => x is not null)
                .Log(this, "Selecting an alias", c => $"Id: {c.Id} - Name: {c.Name}")
                .BindTo(this, vm => vm.SelectedAlias)
                .DisposeWith(d);
            
            // USER SEARCH QUERY 
            this.WhenAnyValue(vm => vm.SearchQuery)
                .DistinctUntilChanged()
                .Throttle(10.Milliseconds(), scheduler: uiThread)
                .Select(x => new SearchRequest(x?.Trim(), AliasToCreate))
                .Log(this, "Invoking search.", c => $"With criterion '{c.Query}' and alias to create '{c.AliasToCreate?.Name ?? "<EMPTY>"}'")
                .InvokeCommand(Search)
                .DisposeWith(d);

            // SAVE OR UPDATE ALIAS
            this.WhenAnyObservable(vm => vm.SaveOrUpdateAlias)
                .Where(x => x is not null)
                .Subscribe(AfterSaveOrUpdateAlias)
                .DisposeWith(d);
        }

        private void AfterSaveOrUpdateAlias(QueryResult updated)
        {
            try
            {
                if (updated is not AliasQueryResult updatedAlias) return;

                using var _ = new LogScope(_logger).Add("UpdatedAlias", updatedAlias)
                                                   .Add("TypeOfQueryResult", updated.GetType())
                                                   .BeginScope();
                // Remove all synonyms ... 
                var toDel = Aliases.Where(a => a.Id == updated.Id);
                Aliases.Remove(toDel);

                // ... and recreate them
                var toAdd = updatedAlias.CloneFromSynonyms();
                Aliases.Add(toAdd);

                // Sort and display selected alias
                Aliases.SortCollection(criterion => criterion.Name);
                SelectedAlias = updatedAlias;
            }
            catch(Exception ex)
            {
                _logger.LogWarning(
                    ex, "An error occured while cloning synonyms of {UpdatedName} after a save or update",
                    updated.Name);
                _notification.Warning($"Error occured while creating synonyms of {updated.Name}");
            }
        }

        private void SetupCommands(IScheduler uiThread, IUserNotification notify, CompositeDisposable d)
        {
            Search = ReactiveCommand
                     .Create<SearchRequest, ObservableCollection<QueryResult>>(OnSearch, outputScheduler: uiThread)
                     .DisposeWith(d);
            Search.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            CreatingAlias = ReactiveCommand.Create(OnCreatingAlias, outputScheduler: uiThread).DisposeWith(d);
            CreatingAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveAlias = ReactiveCommand
                          .CreateFromTask<AliasQueryResult, AliasQueryResult>(OnRemoveAliasAsync, outputScheduler: uiThread)
                          .DisposeWith(d);
            RemoveAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveOrUpdateAlias = ReactiveCommand.Create<AliasQueryResult, QueryResult>(
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