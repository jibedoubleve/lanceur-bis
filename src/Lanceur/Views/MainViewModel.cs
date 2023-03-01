using DynamicData;
using DynamicData.Binding;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Utils;
using Lanceur.Models;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using Lanceur.Xaml;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class MainViewModel : ReactiveObject
    {
        #region Fields

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IDelay _delay;
        private readonly IExecutionManager _executor;
        private readonly IAppLogger _log;
        private readonly SourceList<QueryResult> _results = new();
        private readonly ISearchService _searchService;
        private readonly IDataService _service;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        #endregion Fields

        #region Constructors

        public MainViewModel(
            IScheduler uiThread = null,
            IScheduler poolThread = null,
            IAppLoggerFactory logFactory = null,
            ISearchService searchService = null,
            ICmdlineManager cmdlineService = null,
            IUserNotification notify = null,
            IDataService service = null,
            IExecutionManager executor = null,
            IDelay delay = null)
        {
            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = Locator.Current.GetLogger<MainViewModel>(logFactory);
            _searchService = searchService ?? l.GetService<ISearchService>();
            _cmdlineManager = cmdlineService ?? l.GetService<ICmdlineManager>();
            _delay = delay ?? l.GetService<IDelay>();
            _service = service ?? l.GetService<IDataService>();
            _executor = executor ?? l.GetService<IExecutionManager>();

            //Commands
            AutoComplete = ReactiveCommand.Create(OnAutoComplete, outputScheduler: uiThread);
            AutoComplete.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.CreateFromTask(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SearchAlias = ReactiveCommand.CreateFromTask<string, AliasResponse>(OnSearchAliasAsync, outputScheduler: uiThread);
            SearchAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            //CanExecute for ExecuteAlias
            var isExecutable = this
                    .WhenAnyValue(x => x.CurrentAlias)
                    .Select(x => x != null && x is IExecutable);

            var isSearchFree = SearchAlias
                    .IsExecuting
                    .Select(x => !x);

            var canExecuteAlias = Observable.CombineLatest(
                isExecutable,
                isSearchFree
            ).Where(x => x.Where(y => !y).Any() == false)
             .Select(x => true); ;

            ExecuteAlias = ReactiveCommand.CreateFromTask<ExecutionRequest, AliasResponse>(OnExecuteAliasAsync, canExecuteAlias, outputScheduler: uiThread);
            ExecuteAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectNextResult = ReactiveCommand.CreateFromTask(OnSelectNextResult, outputScheduler: uiThread);
            SelectNextResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectPreviousResult = ReactiveCommand.CreateFromTask(OnSelectPreviousResult, outputScheduler: uiThread);
            SelectPreviousResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            _results
                .Connect()
                .ObserveOn(uiThread)
                .Bind(Results)
                .Subscribe();

            Observable.CombineLatest(
                this.WhenAnyObservable(vm => vm.ExecuteAlias.IsExecuting),
                this.WhenAnyObservable(vm => vm.SearchAlias.IsExecuting)
            ).DistinctUntilChanged()
             .Select(x => x.Where(x => x).Any())
             .BindTo(this, vm => vm.IsBusy);

            this.WhenAnyObservable(
                vm => vm.SelectNextResult,
                vm => vm.SelectPreviousResult
            ).BindTo(this, vm => vm.CurrentAliasSuggestion);

            this.WhenAnyObservable(vm => vm.AutoComplete)
                .Select(x =>
                {
                    var param = _cmdlineManager.BuildFromText(Query);
                    return new Cmdline(x, param.Parameters).ToString();
                })
                .BindTo(this, vm => vm.Query);

            this.WhenAnyValue(vm => vm.Query)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Where(x => !x.IsNullOrWhiteSpace())
                .Select(x => x.Trim())
                .InvokeCommand(SearchAlias);

            this.WhenAnyValue(vm => vm.CurrentAliasIndex)
                .Select(x =>
                {
                    if (Results.Count == 0) { return null; }
                    else if (x == -1 && Results.Count > 0) { return Results.ElementAt(0); }
                    else if (x >= 0 && x < Results.Count) { return Results.ElementAt(x); }
                    else { return null; }
                })
                .BindTo(this, vm => vm.CurrentAlias);

            this.WhenAnyObservable(vm => vm.SearchAlias, vm => vm.ExecuteAlias)
                .Subscribe(x =>
                {
                    _results.Clear();
                    _results.AddRange(x.Results);
                    CurrentAliasIndex = 0;
                    CurrentAliasSuggestion = x.CurrentAliasSuggestion;
                    KeepAlive = x.KeepAlive;
                });

            this.WhenAnyObservable(vm => vm.Activate)
                .Subscribe(x =>
                {
                    _results.Clear();
                    CurrentSessionName = x.CurrentSessionName;
                    CurrentAliasSuggestion = x.CurrentAliasSuggestion;
                });
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, AliasResponse> Activate { get; }

        public ReactiveCommand<Unit, string> AutoComplete { get; }

        [Reactive] public QueryResult CurrentAlias { get; set; }

        [Reactive] public int CurrentAliasIndex { get; set; }

        [Reactive] public string CurrentAliasSuggestion { get; set; }

        [Reactive] public string CurrentSessionName { get; set; }

        public ReactiveCommand<ExecutionRequest, AliasResponse> ExecuteAlias { get; }

        [Reactive] public bool IsBusy { get; set; }

        [Reactive] public bool IsOnError { get; set; }

        [Reactive] public bool KeepAlive { get; set; }

        [Reactive] public string Query { get; set; } = string.Empty;

        public IObservableCollection<QueryResult> Results { get; } = new ObservableCollectionExtended<QueryResult>();

        public ReactiveCommand<string, AliasResponse> SearchAlias { get; }

        public ReactiveCommand<Unit, string> SelectNextResult { get; }

        public ReactiveCommand<Unit, string> SelectPreviousResult { get; }

        #endregion Properties

        #region Methods

        private async Task<AliasResponse> OnActivate()
        {
            var sessionName = await Task.Run(() => _service.GetDefaultSession()?.Name ?? "N.A.");
            return new() { CurrentSessionName = sessionName, };
        }

        private string OnAutoComplete() => CurrentAlias.Name;

        private async Task<AliasResponse> OnExecuteAliasAsync(ExecutionRequest request)
        {
            request ??= new ExecutionRequest();

            if (request?.Query.IsNullOrEmpty() ?? true) { return new(); }
            else
            {
                var cmd = _cmdlineManager.BuildFromText(request.Query);
                if (cmd.Parameters.IsNullOrWhiteSpace() && CurrentAlias is ExecutableQueryResult e)
                {
                    cmd = _cmdlineManager.CloneWithNewParameters(e.Parameters, cmd);
                }

                _log.Debug($"Execute alias '{(request?.Query ?? "<EMPTY>")}'");
                var response = await _executor.ExecuteAsync(new Core.Managers.ExecutionRequest
                {
                    QueryResult = CurrentAlias,
                    Cmdline = cmd,
                    ExecuteWithPrivilege = request.RunAsAdmin,
                });

                _log.Trace($"Execution of '{request.Query}' returned {(response?.Results?.Count() ?? 0)} result(s).");
                return new()
                {
                    Results = response.Results,
                    KeepAlive = response.HasResult
                };
            }
        }

        private async Task<AliasResponse> OnSearchAliasAsync(string criterion)
        {
            if (criterion.IsNullOrWhiteSpace()) { return new AliasResponse(); }
            else
            {
                var query = _cmdlineManager.BuildFromText(criterion);
                var results = await Task.Run(() =>
                {
                    _log.Debug($"Search: criterion '{criterion}'");
                    return _searchService.Search(query)
                        .SetIconForCurrentTheme(isLight: ThemeManager.GetTheme() == ThemeManager.Themes.Light);
                });

                _log.Trace($"Search: Found {results?.Count() ?? 0} element(s)");
                return new()
                {
                    Results = results,
                    KeepAlive = results.Any(),
                    CurrentAliasSuggestion = results.Any() && results.ElementAt(0).IsResult
                        ? results.ElementAt(0)?.Name ?? ""
                        : "",
                };
            }
        }

        private async Task<string> OnSelectNextResult()
        {
            _log.Trace($"Executing '{nameof(OnSelectNextResult)}'");
            if (Results.CanNavigate())
            {
                CurrentAliasSuggestion = String.Empty;
                CurrentAliasIndex = Results.GetNextIndex(CurrentAliasIndex);
                _log.Trace($"Selecting next result. [Index: {CurrentAliasIndex}]");
                await _delay.Of(50);
                return CurrentAlias.ToQuery();
            }
            else { return string.Empty; }
        }

        private async Task<string> OnSelectPreviousResult()
        {
            _log.Trace($"Executing '{nameof(OnSelectPreviousResult)}'");
            if (Results.CanNavigate())
            {
                CurrentAliasSuggestion = String.Empty;
                CurrentAliasIndex = Results.GetPreviousIndex(CurrentAliasIndex);
                _log.Trace($"Selecting previous result. [Index: {CurrentAliasIndex}]");
                await _delay.Of(50);
                return CurrentAlias.ToQuery();
            }
            else { return string.Empty; }
        }

        #endregion Methods

        #region Classes

        public class AliasResponse
        {
            #region Properties

            public string CurrentAliasSuggestion { get; set; }
            public string CurrentSessionName { get; set; }
            public bool KeepAlive { get; set; }
            public IEnumerable<QueryResult> Results { get; set; } = new List<QueryResult>();

            #endregion Properties
        }

        public class ExecutionRequest
        {
            #region Properties

            public string Query { get; set; }

            public bool RunAsAdmin { get; set; }

            #endregion Properties

            #region Methods

            public static implicit operator ExecutionRequest(string parameters) => new() { Query = parameters };

            #endregion Methods
        }

        #endregion Classes
    }
}