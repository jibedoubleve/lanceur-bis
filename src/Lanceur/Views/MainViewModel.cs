using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Requests;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Lanceur.Models;
using Lanceur.Schedulers;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public partial class MainViewModel : ReactiveObject
    {
        #region Fields

        private readonly ISettingsFacade _settingsFacade;
        private readonly ICmdlineManager _cmdlineManager;
        private readonly IDbRepository _dbRepository;
        private readonly IExecutionManager _executor;
        private readonly IAppLogger _log;
        private readonly ISchedulerProvider _schedulers;
        private readonly ISearchService _searchService;

        #endregion Fields

        #region Constructors

        public MainViewModel(
            ISchedulerProvider schedulerProvider = null,
            IAppLoggerFactory logFactory = null,
            ISearchService searchService = null,
            ICmdlineManager cmdlineService = null,
            IUserNotification notify = null,
            IDbRepository dataService = null,
            IExecutionManager executor = null,
            ISettingsFacade appConfigService = null)
        {
            _schedulers = schedulerProvider ?? new RxAppSchedulerProvider();

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = Locator.Current.GetLogger<MainViewModel>(logFactory);
            _searchService = searchService ?? l.GetService<ISearchService>();
            _cmdlineManager = cmdlineService ?? l.GetService<ICmdlineManager>();
            _dbRepository = dataService ?? l.GetService<IDbRepository>();
            _executor = executor ?? l.GetService<IExecutionManager>();
            _settingsFacade = appConfigService ?? l.GetService<ISettingsFacade>();

            #region Commands

            AutoComplete = ReactiveCommand.Create(OnAutoComplete);
            AutoComplete.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: _schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            var canSearch = Query.WhenAnyValue(x => x.IsActive);
            SearchAlias = ReactiveCommand.CreateFromTask<string, AliasResponse>(OnSearchAliasAsync, canSearch, outputScheduler: _schedulers.MainThreadScheduler);
            SearchAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectNextResult = ReactiveCommand.Create(OnSelectNextResult, outputScheduler: _schedulers.MainThreadScheduler);
            SelectNextResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectPreviousResult = ReactiveCommand.Create(OnSelectPreviousResult, outputScheduler: _schedulers.MainThreadScheduler);
            SelectPreviousResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            //CanExecute for ExecuteAlias
            var isExecutable = this
                    .WhenAnyValue(x => x.CurrentAlias)
                    .Select(x => x != null && x is IExecutable);

            var isSearchinished = SearchAlias
                    .IsExecuting
                    .Select(x => !x);

            var canExecuteAlias = Observable.CombineLatest(
                isExecutable,
                isSearchinished
            ).Where(x => x.Where(y => !y).Any() == false)
             .Select(x => true);

            ExecuteAlias = ReactiveCommand.CreateFromTask<AliasExecutionRequest, AliasResponse>(OnExecuteAliasAsync, canExecuteAlias, _schedulers.MainThreadScheduler);
            ExecuteAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            #endregion Commands

            Observable.CombineLatest(
                this.WhenAnyObservable(vm => vm.ExecuteAlias.IsExecuting),
                this.WhenAnyObservable(vm => vm.SearchAlias.IsExecuting)
            ).DistinctUntilChanged()
             .Select(x => x.Where(x => x).Any())
             .Log(this, "ViewModel is busy.", x => $"{x}")
             .ObserveOn(_schedulers.MainThreadScheduler)
             .BindTo(this, vm => vm.IsBusy);

            #region Navigation

            var nav = this.WhenAnyObservable(
                vm => vm.SelectNextResult,
                vm => vm.SelectPreviousResult
            )
             .ObserveOn(_schedulers.MainThreadScheduler)
             .Where(x => x is not null);

            nav.Select(x => x.Result).BindTo(this, vm => vm.CurrentAlias);

            nav.Where(x => x.Result?.Query?.Name is not null)
               .Log(this, "Navigation occured", x => $"Current alias: '{(x?.Result?.Name ?? "<NULL>")}' is {(x.IsActive ? "ACTIVE" : "INACTIVE")}")
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
                .Throttle(TimeSpan.FromMilliseconds(100), _schedulers.TaskpoolScheduler)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Log(this, "Query changed.", x => $"'{x}'")
                .ObserveOn(_schedulers.MainThreadScheduler)
                .InvokeCommand(this, vm => vm.SearchAlias);

            this.WhenAnyValue(vm => vm.Query.Value)
                .Where(x => string.IsNullOrEmpty(x))
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
                .ObserveOn(_schedulers.MainThreadScheduler)
                .BindTo(this, vm => vm.CurrentAlias);

            #region on Search & on Execute

            var obs = this
                .WhenAnyObservable(vm => vm.SearchAlias, vm => vm.ExecuteAlias)
                .ObserveOn(_schedulers.MainThreadScheduler);

            obs.Where(r => (r?.Results?.Count() ?? 0) > 0)
               .Select(r => r?.Results?.ElementAt(0))
               .Log(this, "Command 'ExecuteAlias' or 'SearchAlias' triggered.", x => $"Current alias: '{(x?.Name ?? "<NULL>")}'")
               .BindTo(this, vm => vm.CurrentAlias);

            obs.Select(r => r.Results.ToObservableCollection())
               .Log(this, "New results.", x => $"{x?.Count ?? -1} element(s)")
               .BindTo(this, vm => vm.Results);

            obs.Select(r => r.KeepAlive)
               .ObserveOn(_schedulers.MainThreadScheduler)
               .BindTo(this, vm => vm.KeepAlive);

            #endregion on Search & on Execute

            var activated = this
                .WhenAnyObservable(vm => vm.Activate)
                .DistinctUntilChanged()
                .ObserveOn(_schedulers.MainThreadScheduler);

            activated.Select(x => x.CurrentSessionName)
                     .Log(this, "Activated: get current session name.", x => $"Session name: '{x}'.")
                     .BindTo(this, vm => vm.CurrentSessionName);

            activated.Select(x => x.Results.ToObservableCollection())
                     .Log(this, "Activated: set all results.", x => $"Found {x.Count} item(s).")
                     .BindTo(this, vm => vm.Results);

            activated.Select(x => x.Results)
                     .Where(x => x.Count() > 0)
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
                                select r).SingleOrDefault();
            return currentAlias;
        }

        private AliasResponse OnActivate()
        {
            var sessionName = _dbRepository.GetDefaultSession()?.Name ?? "N.A.";

            var aliases = _settingsFacade.Application.Window.ShowResult
                ? _searchService.GetAll()
                : Array.Empty<AliasQueryResult>();

            aliases.SetIconForCurrentTheme(isLight: ThemeManager.GetTheme() == ThemeManager.Themes.Light);

            _log.Trace($"{(_settingsFacade.Application.Window.ShowResult ? $"View activation: show all {aliases?.Count() ?? 0} result(s)" : "View activation: display nothing")}");
            return new()
            {
                CurrentSessionName = sessionName,
                Results = aliases
            };
        }

        private string OnAutoComplete() => CurrentAlias?.Name ?? string.Empty;

        private async Task<AliasResponse> OnExecuteAliasAsync(AliasExecutionRequest request)
        {
            request ??= new AliasExecutionRequest();

            if (request.AliasToExecute is null) { return new(); }

            var response = await _executor.ExecuteAsync(new ExecutionRequest
            {
                Query = Query,
                QueryResult = request.AliasToExecute,
                ExecuteWithPrivilege = request.RunAsAdmin,
            });

            _log.Trace($"Execution of '{request.Query}' returned {(response?.Results?.Count() ?? 0)} result(s).");
            return new()
            {
                Results = response?.Results ?? Array.Empty<QueryResult>(),
                KeepAlive = response?.HasResult ?? false
            };
        }

        private Task<AliasResponse> OnSearchAliasAsync(string criterion)
        {
            var showResult = _settingsFacade.Application.Window.ShowResult;
            if (criterion.IsNullOrWhiteSpace() && showResult) { return new AliasResponse(); }
            if (criterion.IsNullOrWhiteSpace() && !showResult)
            {
                var all = _searchService.GetAll().SetIconForCurrentTheme(isLight: ThemeManager.GetTheme() == ThemeManager.Themes.Light);
                return new AliasResponse
                {
                    Results = all,
                    KeepAlive = all.Any()
                };
            }

            var query = _cmdlineManager.BuildFromText(criterion);

            _log.Debug($"Search: criterion '{criterion}'");

            var results = _searchService
                    .Search(query)
                    .SetIconForCurrentTheme(isLight: ThemeManager.GetTheme() == ThemeManager.Themes.Light);

            _log.Trace($"Search: Found {results?.Count() ?? 0} element(s)");
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
            _log.Trace($"Selecting next result. [Index: {nextAlias?.Name}]");

            return NavigationResponse.InactiveFromResult(nextAlias);
        }

        private NavigationResponse OnSelectPreviousResult()
        {
            if (!Results.CanNavigate()) { return null; }

            var currentIndex = Results.IndexOf(GetCurrentAlias());
            var previousAlias = Results.GetPreviousItem(currentIndex);
            _log.Trace($"Selecting previous result. [Index: {previousAlias?.Name}]");

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