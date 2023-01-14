using DynamicData;
using DynamicData.Binding;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Models;
using Lanceur.SharedKernel;
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
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class MainViewModel : ReactiveObject
    {
        #region Fields

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IDelay _delay;
        private readonly IExecutionManager _executor;
        private readonly ILogService _log;
        private readonly SourceList<QueryResult> _results = new();
        private readonly ISearchService _searchService;
        private readonly IDataService _service;

        #endregion Fields

        #region Constructors

        public MainViewModel(
            IScheduler uiThread = null,
            IScheduler poolThread = null,
            ILogService log = null,
            ISearchService searchService = null,
            ICmdlineManager cmdlineService = null,
            IUserNotification notify = null,
            IDataService service = null,
            IExecutionManager executor = null,
            IDelay delay = null)
        {
            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            IsBusyScope = new Scope<bool>(x => IsBusy = x, true, false);

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = log ?? Locator.Current.GetService<ILogService>();
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
            Activate = ReactiveCommand.CreateFromTask(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SearchAlias = ReactiveCommand.Create<string, SearchContext>(OnSearchAlias, outputScheduler: uiThread);
            SearchAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            ExecuteAlias = ReactiveCommand.CreateFromTask<ExecutionContext, SearchContext>(OnExecuteAliasAsync, canExecuteAlias, outputScheduler: uiThread);
            ExecuteAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectNextResult = ReactiveCommand.CreateFromTask(OnSelectNextResult, outputScheduler: uiThread);
            SelectNextResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectPreviousResult = ReactiveCommand.CreateFromTask(OnSelectPreviousResult, outputScheduler: uiThread);
            SelectPreviousResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            AutoCompleteQuery = ReactiveCommand.Create(OnAutoCompleteQuery, outputScheduler: uiThread);
            AutoCompleteQuery.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            _results
                .Connect()
                .ObserveOn(uiThread)
                .Bind(Results)
                .Subscribe();

            this.WhenAnyValue(x => x.Query)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Where(x => !IsBusy)
                .Select(x => x.Trim())
                .Log(this, $"Query changed", s => $"Query: {s}")
                .InvokeCommand(SearchAlias);

            this.WhenAnyValue(x => x.CurrentAliasIndex)
                .DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Select(x =>
                {
                    if (Results.Count == 0) { return null; }
                    else if (x == -1 && Results.Count > 0) { return Results.ElementAt(0); }
                    else if (x >= 0 && x < Results.Count) { return Results.ElementAt(x); }
                    else { return null; }
                })
                .BindTo(this, x => x.CurrentAlias);

            this.WhenAnyObservable(vm => vm.SearchAlias, vm => vm.ExecuteAlias)
                .Subscribe(x =>
                {
                    _results.Clear();
                    _results.AddRange(x.Results);
                    CurrentAliasIndex = 0;
                    CurrentAliasSuggestion = x.CurrentAliasSuggestion;
                });

            this.WhenAnyObservable(vm => vm.Activate)
                .Select(x => x.CurrentSessionName)
                .BindTo(this, vm => vm.CurrentSessionName);

            this.WhenAnyObservable(vm => vm.AutoCompleteQuery)
                .ObserveOn(uiThread)
                .BindTo(this, vm => vm.Query);
        }

        #endregion Constructors

        #region Properties

        private Scope<bool> IsBusyScope { get; init; }

        public ReactiveCommand<Unit, SearchContext> Activate { get; }

        public ReactiveCommand<Unit, string> AutoCompleteQuery { get; }

        [Reactive] public QueryResult CurrentAlias { get; set; }

        [Reactive] public int CurrentAliasIndex { get; set; }

        [Reactive] public string CurrentAliasSuggestion { get; set; }

        [Reactive] public string CurrentSessionName { get; set; }

        public ReactiveCommand<ExecutionContext, SearchContext> ExecuteAlias { get; }

        [Reactive] public bool IsBusy { get; set; }
        [Reactive] public bool IsOnError { get; set; }
        [Reactive] public bool KeepAlive { get; set; }

        [Reactive] public string Query { get; set; } = string.Empty;

        public IObservableCollection<QueryResult> Results { get; } = new ObservableCollectionExtended<QueryResult>();

        public ReactiveCommand<string, SearchContext> SearchAlias { get; }

        public ReactiveCommand<Unit, Unit> SelectNextResult { get; }

        public ReactiveCommand<Unit, Unit> SelectPreviousResult { get; }

        #endregion Properties

        #region Methods

        private async Task<SearchContext> OnActivate()
        {
            //Clear the previous results...
            Results.Clear();
            CurrentAliasSuggestion = string.Empty;

            var sessionName = await Task.Run(() => _service.GetDefaultSession()?.Name ?? "N.A.");

            return new()
            {
                CurrentSessionName = sessionName
            };
        }

        private string OnAutoCompleteQuery()
        {
            var @params = _cmdlineManager.BuildFromText(Query);
            var alias = Results?.FirstOrDefault();

            return alias?.IsResult ?? false
                ? $"{alias?.Name} {@params?.Parameters}"
                : Query.ToString();
        }

        private async Task<SearchContext> OnExecuteAliasAsync(ExecutionContext context)
        {
            context ??= new ExecutionContext();

            using (IsBusyScope.Open())
            {
                if (context?.Query.IsNullOrEmpty() ?? true) { return new(); }
                else
                {
                    var cmd = _cmdlineManager.BuildFromText(context.Query);
                    if (cmd.Parameters.IsNullOrWhiteSpace() && CurrentAlias is ExecutableQueryResult e)
                    {
                        cmd = _cmdlineManager.CloneWithNewParameters(e.Parameters, cmd);
                    }

                    _log.Debug($"Execute alias '{CurrentAlias.ToQuery()}'");
                    var response = await _executor.ExecuteAsync(new ExecutionRequest
                    {
                        QueryResult = CurrentAlias,
                        Cmdline = cmd,
                        ExecuteWithPrivilege = context.RunAsAdmin,
                    });
                    KeepAlive = response.HasResult;

                    // A small delay to be sure the query changes are not handled after we
                    // return the result. Without delay, we can encounter result override
                    // and see the result of an outdated query
                    await _delay.Of(50);
                    return new() { Results = response.Results };
                }
            }
        }

        private SearchContext OnSearchAlias(string criterion)
        {
            if (criterion.IsNullOrWhiteSpace()) { return new(); }
            else
            {
                using (IsBusyScope.Open())
                {
                    _log.Trace($"Search: criterion '{criterion}'");

                    var query = _cmdlineManager.BuildFromText(criterion);
                    var results = _searchService.Search(query)
                                                .SetIconForCurrentTheme(isLight: ThemeHelper.IsLightTheme());

                    _log.Trace($"Search: criterion '{criterion}' found {results?.Count() ?? 0} element(s)");
                    return new()
                    {
                        Results = results,
                        CurrentAliasSuggestion = results.Any() && results.ElementAt(0).IsResult
                            ? results.ElementAt(0)?.Name ?? ""
                            : "",
                    };
                }
            }
        }

        private async Task OnSelectNextResult()
        {
            if (Results.CanNavigate())
            {
                using (IsBusyScope.Open())
                {
                    CurrentAliasSuggestion = String.Empty;
                    CurrentAliasIndex = Results.GetNextIndex(CurrentAliasIndex);
                    _log.Trace($"Selecting next result. [Index: {CurrentAliasIndex}]");
                    Query = CurrentAlias.ToQuery();
                    await _delay.Of(50);
                }
            }
        }

        private async Task OnSelectPreviousResult()
        {
            if (Results.CanNavigate())
            {
                using (IsBusyScope.Open())
                {
                    CurrentAliasSuggestion = String.Empty;
                    CurrentAliasIndex = Results.GetPreviousIndex(CurrentAliasIndex);
                    _log.Trace($"Selecting previous result. [Index: {CurrentAliasIndex}]");
                    Query = CurrentAlias.ToQuery();
                    await _delay.Of(50);
                }
            }
        }

        #endregion Methods

        #region Classes

        public class ExecutionContext
        {
            #region Properties

            public string Query { get; set; }

            public bool RunAsAdmin { get; set; }

            #endregion Properties

            #region Methods

            public static implicit operator ExecutionContext(string parameters) => new() { Query = parameters };

            #endregion Methods
        }

        public class SearchContext
        {
            #region Properties

            public string CurrentAliasSuggestion { get; set; }
            public string CurrentSessionName { get; set; }
            public IEnumerable<QueryResult> Results { get; set; } = new List<QueryResult>();

            #endregion Properties
        }

        #endregion Classes
    }
}