using DynamicData;
using DynamicData.Binding;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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
    public class KeywordsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly SourceList<QueryResult> _aliases = new();
        private readonly IThumbnailManager _thumbnailManager;
        private readonly IDataService _aliasService;
        private readonly Interaction<string, bool> _confirmRemove;
        private readonly ILogService _log;

        #endregion Fields

        #region Constructors

        public KeywordsViewModel(
            ILogService logService = null,
            IDataService searchService = null,
            IScheduler uiThread = null,
            IScheduler poolThread = null,
            IUserNotification notify = null,
            IThumbnailManager thumbnailManager = null)
        {
            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = logService ?? l.GetService<ILogService>(); ;
            _thumbnailManager = thumbnailManager ?? l.GetService<IThumbnailManager>();
            _aliasService = searchService ?? l.GetService<IDataService>();
            _confirmRemove = Interactions.YesNoQuestion(uiThread);

            Activate = ReactiveCommand.Create(OnActivated, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Search = ReactiveCommand.Create<string, IEnumerable<QueryResult>>(OnSearch, outputScheduler: uiThread);
            Search.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            DuplicateAlias = ReactiveCommand.Create<Unit, AliasQueryResult>(OnDuplicateAlias, outputScheduler: uiThread);
            DuplicateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            CreateAlias = ReactiveCommand.Create<string, AliasQueryResult>(OnCreateAlias, outputScheduler: uiThread);
            CreateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            RemoveAlias = ReactiveCommand.CreateFromTask<AliasQueryResult, Unit>(OnRemoveAliasAsync, outputScheduler: uiThread);
            RemoveAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveOrUpdateAlias = ReactiveCommand.Create<AliasQueryResult, Unit>(OnSaveOrUpdateAlias, outputScheduler: uiThread);
            SaveOrUpdateAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            _aliases
                .Connect()
                .ObserveOn(uiThread)
                .Bind(Aliases)
                .Subscribe();

            this.WhenAnyObservable(vm => vm.Activate)
                .DistinctUntilChanged()
                .Log(this, "Activate command triggered")
                .Subscribe(SetAliases);

            this.WhenAnyObservable(vm => vm.Search)
                .DistinctUntilChanged()
                .Log(this, "Search command triggered")
                .Subscribe(SetAliases);

            this.WhenAnyObservable(vm => vm.DuplicateAlias)
                .Select(x =>
                {
                    _log.Trace($"Duplicated alias '{x.Name}'");
                    _aliases.Add(x);
                    return x;
                })
                .BindTo(this, vm => vm.SelectedAlias);

            this.WhenAnyObservable(vm => vm.CreateAlias)
                .Select(x =>
                {
                    _log.Trace($"Creating alias '{x.Name}'");
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

        public ReactiveCommand<Unit, IEnumerable<QueryResult>> Activate { get; set; }

        public IObservableCollection<QueryResult> Aliases { get; } = new ObservableCollectionExtended<QueryResult>();

        public Interaction<string, bool> ConfirmRemove => _confirmRemove;

        public ReactiveCommand<string, AliasQueryResult> CreateAlias { get; }

        public ReactiveCommand<Unit, AliasQueryResult> DuplicateAlias { get; }

        public ReactiveCommand<AliasQueryResult, Unit> RemoveAlias { get; }

        public ReactiveCommand<AliasQueryResult, Unit> SaveOrUpdateAlias { get; }

        public ReactiveCommand<string, IEnumerable<QueryResult>> Search { get; }

        [Reactive] public string SearchQuery { get; set; }

        [Reactive] public AliasQueryResult SelectedAlias { get; set; }

        #endregion Properties

        #region Methods

        private IEnumerable<QueryResult> OnActivated()
        {
            var results = _aliasService.GetAll();
            _thumbnailManager.RefreshThumbnails(results);
            return results;
        }

        private AliasQueryResult OnCreateAlias(string aliasName)
        {
            return aliasName.IsNullOrWhiteSpace()
                ? AliasQueryResult.EmptyForCreation
                : AliasQueryResult.FromName(aliasName);
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

        private Unit OnSaveOrUpdateAlias(AliasQueryResult alias)
        {
            var created = alias.Id == 0;

            _aliasService.SaveOrUpdate(ref alias);
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
            SelectedAlias = null;
            _aliases.Clear();
            _aliases.AddRange(x);

        }

        #endregion Methods
    }
}