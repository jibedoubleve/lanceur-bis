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
using NLog.Targets;
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
using System.Windows.Input;

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

            //Command CanExecute
            var canExecuteAlias = this
                .WhenAnyValue(x => x.CurrentAlias)
                .Where(x => x != null && x is IExecutable)
                .Select(x => true);

            //Commands
            AutoComplete = ReactiveCommand.Create(OnAutoComplete, outputScheduler: uiThread);
            AutoComplete.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.CreateFromTask(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SearchAlias = ReactiveCommand.CreateFromTask<string, SearchResponse>(OnSearchAliasAsync, outputScheduler: uiThread);
            SearchAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            ExecuteAlias = ReactiveCommand.CreateFromTask<ExecutionRequest, SearchResponse>(OnExecuteAliasAsync, canExecuteAlias, outputScheduler: uiThread);
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
                this.WhenAnyObservable(vm => vm.SearchAlias.IsExecuting),
                this.WhenAnyObservable(vm => vm.SelectNextResult.IsExecuting),
                this.WhenAnyObservable(vm => vm.SelectPreviousResult.IsExecuting))
                .DistinctUntilChanged()
                .Select(x => x.Where(x => x).Any())
                .Log(this, $"IsBusy changed.", s => $"New value: {s}.")
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
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Where(x => !IsBusy)
                .Select(x => x.Trim())
                .Log(this, $"Query changed", s => $"Query: {s}")
                .InvokeCommand(SearchAlias);

            this.WhenAnyValue(vm => vm.CurrentAliasIndex)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
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

        private string OnAutoComplete() => CurrentAlias.Name;

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, SearchResponse> Activate { get; }

        public ReactiveCommand<Unit, string> AutoComplete { get; }

        [Reactive] public QueryResult CurrentAlias { get; set; }

        [Reactive] public int CurrentAliasIndex { get; set; }

        [Reactive] public string CurrentAliasSuggestion { get; set; }

        [Reactive] public string CurrentSessionName { get; set; }

        public ReactiveCommand<ExecutionRequest, SearchResponse> ExecuteAlias { get; }

        [Reactive] public bool IsBusy { get; set; }
        [Reactive] public bool IsOnError { get; set; }
        [Reactive] public bool KeepAlive { get; set; }

        [Reactive] public string Query { get; set; } = string.Empty;

        public IObservableCollection<QueryResult> Results { get; } = new ObservableCollectionExtended<QueryResult>();

        public ReactiveCommand<string, SearchResponse> SearchAlias { get; }

        public ReactiveCommand<Unit, string> SelectNextResult { get; }

        public ReactiveCommand<Unit, string> SelectPreviousResult { get; }

        #endregion Properties

        #region Methods

        private async Task<SearchResponse> OnActivate()
        {
            var sessionName = await Task.Run(() => _service.GetDefaultSession()?.Name ?? "N.A.");
            return new() { CurrentSessionName = sessionName, };
        }

        private async Task<SearchResponse> OnExecuteAliasAsync(ExecutionRequest request)
        {
            _log.Trace($"Executing '{nameof(OnExecuteAliasAsync)}'");
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

                // A small delay to be sure the query changes are not handled after we
                // return the result. Without delay, we can encounter result override
                // and see the result of an outdated query
                await _delay.Of(50);
                return new()
                {
                    Results = response.Results,
                    KeepAlive = response.HasResult
                };
            }
        }

        private async Task<SearchResponse> OnSearchAliasAsync(string criterion)
        {
            _log.Trace($"Executing '{nameof(OnSearchAliasAsync)}'");
            if (criterion.IsNullOrWhiteSpace()) { return new SearchResponse(); }
            else
            {

                var query = _cmdlineManager.BuildFromText(criterion);
                var results = await Task.Run(() =>
                {
                    _log.Trace($"Search: criterion '{criterion}'");
                    return _searchService.Search(query)
                        .SetIconForCurrentTheme(isLight: ThemeManager.GetTheme() == ThemeManager.Themes.Light);
                });

                _log.Trace($"Search: criterion '{criterion}' found {results?.Count() ?? 0} element(s)");
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

        public class SearchResponse
        {
            #region Properties

            public string CurrentAliasSuggestion { get; set; }
            public string CurrentSessionName { get; set; }
            public bool KeepAlive { get; set; }
            public IEnumerable<QueryResult> Results { get; set; } = new List<QueryResult>();

            #endregion Properties
        }

        #endregion Classes
    }
}