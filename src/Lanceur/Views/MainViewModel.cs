using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Schedulers;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Humanizer;

namespace Lanceur.Views
{
    public partial class MainViewModel : ReactiveObject
    {
        #region Fields

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IDbRepository _dbRepository;
        private readonly IExecutionManager _executor;
        private readonly ILogger<MainViewModel> _logger;
        private readonly ISearchService _searchService;
        private readonly ISettingsFacade _settingsFacade;

        #endregion Fields

        #region Constructors

        public MainViewModel(
            ISchedulerProvider schedulerProvider = null,
            ILoggerFactory loggerFactory = null,
            ISearchService searchService = null,
            ICmdlineManager cmdlineService = null,
            IUserNotification notify = null,
            IDbRepository dataService = null,
            IExecutionManager executor = null,
            ISettingsFacade appConfigService = null)
        {
            schedulerProvider ??= new RxAppSchedulerProvider();

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _logger = loggerFactory.GetLogger<MainViewModel>();
            _searchService = searchService ?? l.GetService<ISearchService>();
            _cmdlineManager = cmdlineService ?? l.GetService<ICmdlineManager>();
            _dbRepository = dataService ?? l.GetService<IDbRepository>();
            _executor = executor ?? l.GetService<IExecutionManager>();
            _settingsFacade = appConfigService ?? l.GetService<ISettingsFacade>();

            #region Commands

            AutoComplete = ReactiveCommand.Create(OnAutoComplete);
            AutoComplete.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: schedulerProvider.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            var canSearch = Query.WhenAnyValue(x => x.IsActive);
            SearchAlias = ReactiveCommand.CreateFromTask<string, AliasResponse>(OnSearchAliasAsync, canSearch, outputScheduler: schedulerProvider.MainThreadScheduler);
            SearchAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectNextResult = ReactiveCommand.Create(OnSelectNextResult, outputScheduler: schedulerProvider.MainThreadScheduler);
            SelectNextResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectPreviousResult = ReactiveCommand.Create(OnSelectPreviousResult, outputScheduler: schedulerProvider.MainThreadScheduler);
            SelectPreviousResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            //CanExecute for ExecuteAlias
            var isExecutable = this
                    .WhenAnyValue(x => x.CurrentAlias)
                    .Select(x => x is IExecutable);

            var isSearchFinished = SearchAlias
                    .IsExecuting
                    .Select(x => !x);

            var canExecuteAlias = Observable.CombineLatest(
                isExecutable,
                isSearchFinished
            ).Where(x => x.Any(y => !y) == false)
             .Select(_ => true);

            ExecuteAlias = ReactiveCommand.CreateFromTask<AliasExecutionRequest, AliasResponse>(OnExecuteAliasAsync, canExecuteAlias, schedulerProvider.MainThreadScheduler);
            ExecuteAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            #endregion Commands

            Observable.CombineLatest(
                this.WhenAnyObservable(vm => vm.ExecuteAlias.IsExecuting),
                this.WhenAnyObservable(vm => vm.SearchAlias.IsExecuting)
            )
                      .DistinctUntilChanged()
                      .Select(x => x.Any(x => x))
                      .Log(this, "ViewModel is busy.", x => $"{x}")
                      .ObserveOn(schedulerProvider.MainThreadScheduler)
                      .BindTo(this, vm => vm.IsBusy);

            #region Navigation

            var nav = this.WhenAnyObservable(
                vm => vm.SelectNextResult,
                vm => vm.SelectPreviousResult
            )
             .ObserveOn(schedulerProvider.MainThreadScheduler)
             .Where(x => x is not null);

            nav.Select(x => x.Result).BindTo(this, vm => vm.CurrentAlias);

            nav.Where(x => x.Result?.Query?.Name is not null)
               .Log(this, "Navigation occured",
                    x => $"Current alias: '{(x?.Result?.Name ?? "<NULL>")}' is {(x?.IsActive ?? false ? "ACTIVE" : "INACTIVE")}")
               .Subscribe(x =>
               {
                   Query.IsActive = x.IsActive;
                   Query.Value = x.Result.Name;
               });

            #endregion Navigation

            this.WhenAnyObservable(vm => vm.AutoComplete)
                .Select(x =>
                {
                    var param = _cmdlineManager.BuildFromText(Query);
                    return new Cmdline(x, param.Parameters).ToString();
                })
                .BindTo(Query, query => query.Value);

            #region Query

            this.WhenAnyValue(vm => vm.Query.Value)
                .Throttle(100.Milliseconds(), schedulerProvider.TaskpoolScheduler)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Log(this, "Query changed.", x => $"'{x}'")
                .ObserveOn(schedulerProvider.MainThreadScheduler)
                .InvokeCommand(this, vm => vm.SearchAlias);

            this.WhenAnyValue(vm => vm.Query.Value)
                .Where(string.IsNullOrEmpty)
                .Log(this, "Query is empty, clearing the view.", x => $"'{x}'")
                .Subscribe(_ =>
                {
                    Query.Value = string.Empty;
                    Query.IsActive = true;
                    CurrentAlias = null;
                    Results.Clear();
                });

            #endregion Query

            this.WhenAnyValue(vm => vm.Results)
                .Where(x => x is not null)
                .Select(x => x.FirstOrDefault())
                .Log(this, "Results changed.", x => $"Current alias: '{(x?.Name ?? "<NULL>")}'")
                .ObserveOn(schedulerProvider.MainThreadScheduler)
                .BindTo(this, vm => vm.CurrentAlias);

            #region on Search & on Execute

            var obs = this
                .WhenAnyObservable(vm => vm.SearchAlias, vm => vm.ExecuteAlias)
                .ObserveOn(schedulerProvider.MainThreadScheduler);

            obs.Where(r => (r?.Results?.Count() ?? 0) > 0)
               .Select(r => r?.Results?.ElementAt(0))
               .Log(this, "Command 'ExecuteAlias' or 'SearchAlias' triggered.", x => $"Current alias: '{(x?.Name ?? "<NULL>")}'")
               .BindTo(this, vm => vm.CurrentAlias);

            obs.Select(r => r.Results.ToObservableCollection())
               .Log(this, "New results.", x => $"{x?.Count ?? -1} element(s)")
               .BindTo(this, vm => vm.Results);

            obs.Select(r => r.KeepAlive)
               .ObserveOn(schedulerProvider.MainThreadScheduler)
               .BindTo(this, vm => vm.KeepAlive);

            #endregion on Search & on Execute

            var activated = this
                .WhenAnyObservable(vm => vm.Activate)
                .DistinctUntilChanged()
                .ObserveOn(schedulerProvider.MainThreadScheduler);

            activated.Select(x => x.CurrentSessionName)
                     .Log(this, "Activated: get current session name.", x => $"Session name: '{x}'.")
                     .BindTo(this, vm => vm.CurrentSessionName);

            activated.Select(x => x.Results.ToObservableCollection())
                     .Log(this, "Activated: set all results.", x => $"Found {x.Count} item(s).")
                     .BindTo(this, vm => vm.Results);

            activated.Select(x => x.Results)
                     .Where(x => x.Any())
                     .Select(x => x.First())
                     .Log(this, "Activated: Select current alias.", x => $"Current alias is '{x.Name}'.")
                     .BindTo(this, vm => vm.CurrentAlias);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, AliasResponse> Activate { get; }
        public ReactiveCommand<Unit, string> AutoComplete { get; }
        [Reactive] public QueryResult CurrentAlias { get; set; }
        [Reactive] public string CurrentSessionName { get; set; }
        public ReactiveCommand<AliasExecutionRequest, AliasResponse> ExecuteAlias { get; }
        [Reactive] public bool IsBusy { get; set; }
        [Reactive] public bool IsOnError { get; set; }
        [Reactive] public bool KeepAlive { get; set; }
        public QueryViewModel Query { get; init; } = QueryViewModel.Empty;
        [Reactive] public ObservableCollection<QueryResult> Results { get; set; } = new();
        public ReactiveCommand<string, AliasResponse> SearchAlias { get; }
        public ReactiveCommand<Unit, NavigationResponse> SelectNextResult { get; }
        public ReactiveCommand<Unit, NavigationResponse> SelectPreviousResult { get; }
        [Reactive] public string Suggestion { get; set; }

        #endregion Properties

        #region Methods

        private QueryResult GetCurrentAlias()
        {
            var hash = CurrentAlias?.GetHashCode() ?? 0;
            var currentAlias = (from r in Results
                                where r.GetHashCode() == hash
                                select r).ToArray();

            return currentAlias.SingleOrDefault();
        }

        private AliasResponse OnActivate()
        {
            var sessionName = _dbRepository.GetDefaultSession()?.Name ?? "N.A.";

            var aliases = _settingsFacade.Application.Window.ShowResult
                ? _searchService.GetAll().ToArray()
                : Array.Empty<AliasQueryResult>();

            _logger.LogInformation(
                "MainViewModel activated. There is {Length} alias(es) to show. (Show all results on search box show: {ShowResult})",
                aliases.Length, _settingsFacade.Application.Window.ShowResult);
            return new()
            {
                CurrentSessionName = sessionName,
                Results = aliases
            };
        }

        private string OnAutoComplete() => CurrentAlias?.Name ?? string.Empty;

        private async Task<AliasResponse> OnExecuteAliasAsync(AliasExecutionRequest request)
        {
            request ??= new();
            using var _ = _logger.BeginSingleScope("ExecuteAliasRequest", request);

            if (request.AliasToExecute is null) { return new(); }

            var response = await _executor.ExecuteAsync(new()
            {
                Query = Query,
                QueryResult = request.AliasToExecute,
                ExecuteWithPrivilege = request.RunAsAdmin,
            });

            return new()
            {
                Results = response.Results ?? Array.Empty<QueryResult>(),
                KeepAlive = response.HasResult
            };
        }

        private Task<AliasResponse> OnSearchAliasAsync(string criterion)
        {
            var showResult = _settingsFacade.Application.Window.ShowResult;
            if (criterion.IsNullOrWhiteSpace() && showResult) { return new AliasResponse(); }
            if (criterion.IsNullOrWhiteSpace() && !showResult)
            {
                var all = _searchService.GetAll()
                                        .ToArray();
                return new AliasResponse
                {
                    Results = all,
                    KeepAlive = all.Any()
                };
            }

            var query = _cmdlineManager.BuildFromText(criterion);

            _logger.LogInformation("Search criterion: {Criterion}", criterion);

            var results = _searchService.Search(query)
                                        .ToArray();

            _logger.LogDebug("Search: Found {Length} element(s)", results.Length);
            return new AliasResponse()
            {
                Results = results,
                KeepAlive = results.Any()
            };
        }

        private NavigationResponse OnSelectNextResult()
        {
            if (!Results.CanNavigate()) { return null; }

            var currentIndex = Results.IndexOf(GetCurrentAlias());
            var nextAlias = Results.GetNextItem(currentIndex);
            _logger.LogTrace("Selecting next result. [Index: {Name}]", nextAlias.Name);

            return NavigationResponse.InactiveFromResult(nextAlias);
        }

        private NavigationResponse OnSelectPreviousResult()
        {
            if (!Results.CanNavigate()) { return null; }

            var currentIndex = Results.IndexOf(GetCurrentAlias());
            var previousAlias = Results.GetPreviousItem(currentIndex);
            _logger.LogTrace("Selecting previous result. [Index: {Name}]", previousAlias.Name);

            return NavigationResponse.InactiveFromResult(previousAlias);
        }

        #endregion Methods

        #region Classes

        public class NavigationResponse
        {
            #region Constructors

            private NavigationResponse(QueryResult result, bool isActive) => (Result, IsActive) = (result, isActive);

            #endregion Constructors

            #region Properties

            public bool IsActive { get; init; }

            public QueryResult Result { get; init; }

            #endregion Properties

            #region Methods

            public static NavigationResponse InactiveFromResult(QueryResult result) => new(result, false);

            #endregion Methods
        }

        #endregion Classes
    }
}