using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
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

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IExecutionManager _executor;
        private readonly IAppLogger _log;
        private readonly ISchedulerProvider _schedulers;
        private readonly ISearchService _searchService;
        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public MainViewModel(
            ISchedulerProvider schedulerProvider = null,
            IAppLoggerFactory logFactory = null,
            ISearchService searchService = null,
            ICmdlineManager cmdlineService = null,
            IUserNotification notify = null,
            IDataService service = null,
            IExecutionManager executor = null)
        {
            _schedulers = schedulerProvider ?? new RxAppSchedulerProvider();

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = Locator.Current.GetLogger<MainViewModel>(logFactory);
            _searchService = searchService ?? l.GetService<ISearchService>();
            _cmdlineManager = cmdlineService ?? l.GetService<ICmdlineManager>();
            _service = service ?? l.GetService<IDataService>();
            _executor = executor ?? l.GetService<IExecutionManager>();

            #region Commands

            AutoComplete = ReactiveCommand.Create(OnAutoComplete);
            AutoComplete.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.CreateFromTask(OnActivate, outputScheduler: _schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SearchAlias = ReactiveCommand.CreateFromTask<string, AliasResponse>(OnSearchAliasAsync, outputScheduler: _schedulers.MainThreadScheduler);
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
            ).Log(this, "Navigation occured", x => $"Current alias: '{(x?.Name ?? "<NULL>")}'")
             .ObserveOn(_schedulers.MainThreadScheduler)
             .Select(x => x).BindTo(this, vm => vm.CurrentAlias);

            #endregion Navigation

            this.WhenAnyObservable(vm => vm.AutoComplete)
                .Select(x =>
                {
                    var param = _cmdlineManager.BuildFromText(Query);
                    return new Cmdline(x, param.Parameters).ToString();
                })
                .BindTo(this, vm => vm.Query);

            this.WhenAnyValue(vm => vm.Query)
                .Throttle(TimeSpan.FromMilliseconds(100), _schedulers.TaskpoolScheduler)
                .Select(x => x.Trim())
                .Where(x => !x.IsNullOrWhiteSpace())
                .Log(this, "Query changed.", x => $"'{x}'")
                .ObserveOn(_schedulers.MainThreadScheduler)
                .InvokeCommand(this, vm => vm.SearchAlias);

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

            obs.Select(r => r?.Results?.ElementAt(0))
                .Log(this, "Command 'ExecuteAlias' or 'SearchAlias' triggered.", x => $"Current alias: '{(x?.Name ?? "<NULL>")}'")
                .BindTo(this, vm => vm.CurrentAlias);

            obs.Select(r => r.Results.ToObservableCollection())
                .Log(this, "New results.", x => $"{x?.Count ?? -1} element(s)")
                .BindTo(this, vm => vm.Results);

            obs.Select(r => r.KeepAlive)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .BindTo(this, vm => vm.KeepAlive);

            #endregion on Search & on Execute

            this.WhenAnyObservable(vm => vm.Activate)
                .Subscribe(x => CurrentSessionName = x.CurrentSessionName);
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

        [Reactive] public string Query { get; set; } = string.Empty;

        [Reactive] public ObservableCollection<QueryResult> Results { get; set; } = new();

        public ReactiveCommand<string, AliasResponse> SearchAlias { get; }

        public ReactiveCommand<Unit, QueryResult> SelectNextResult { get; }

        public ReactiveCommand<Unit, QueryResult> SelectPreviousResult { get; }

        #endregion Properties

        #region Methods

        private async Task<AliasResponse> OnActivate()
        {
            _log.Info("Activating ViewModel");
            var sessionName = await Task.Run(() => _service.GetDefaultSession()?.Name ?? "N.A.");
            return new() { CurrentSessionName = sessionName, };
        }

        private string OnAutoComplete() => CurrentAlias?.Name ?? string.Empty;

        private async Task<AliasResponse> OnExecuteAliasAsync(AliasExecutionRequest request)
        {
            request ??= new AliasExecutionRequest();

            if (request?.Query.IsNullOrEmpty() ?? true) { return new(); }
            if (CurrentAlias is null) { return new(); }

            _log.Debug($"Execute alias '{(request?.Query ?? "<EMPTY>")}'");

            var response = await _executor.ExecuteAsync(new ExecutionRequest
            {
                Query = Query,
                QueryResult = CurrentAlias,
                ExecuteWithPrivilege = request.RunAsAdmin,
            });

            _log.Trace($"Execution of '{request.Query}' returned {(response?.Results?.Count() ?? 0)} result(s).");
            return new()
            {
                Results = response.Results,
                KeepAlive = response.HasResult
            };
        }

        private Task<AliasResponse> OnSearchAliasAsync(string criterion)
        {
            if (criterion.IsNullOrWhiteSpace()) { return new AliasResponse(); }
            else
            {
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
        }

        private QueryResult OnSelectNextResult()
        {
            if (!Results.CanNavigate()) { return null; }

            var currentIndex = Results.IndexOf(CurrentAlias);
            var nextAlias = Results.GetNextItem(currentIndex);
            _log.Trace($"Selecting next result. [Index: {nextAlias?.Name}]");

            return nextAlias;
        }

        private QueryResult OnSelectPreviousResult()
        {
            if (!Results.CanNavigate()) { return null; }

            var currentIndex = Results.IndexOf(CurrentAlias);
            var previousAlias = Results.GetPreviousItem(currentIndex);
            _log.Trace($"Selecting previous result. [Index: {previousAlias?.Name}]");

            return previousAlias;
        }

        #endregion Methods
    }
}