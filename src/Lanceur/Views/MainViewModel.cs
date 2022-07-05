using DynamicData;
using DynamicData.Binding;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Mixins;
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
            IDataService service = null)
        {
            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = log ?? Locator.Current.GetService<ILogService>();
            _searchService = searchService ?? l.GetService<ISearchService>();
            _cmdlineManager = cmdlineService ?? l.GetService<ICmdlineManager>();
            _service = service ?? l.GetService<IDataService>();

            //Command CanExecute
            var canExecuteAlias = this
                .WhenAnyValue(x => x.CurrentAlias)
                .Where(x => x != null && x is IExecutable)
                .Select(x => true);

            //Commands
            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: uiThread);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SearchAlias = ReactiveCommand.Create<string, SearchContext>(OnSearchAlias, outputScheduler: uiThread);
            SearchAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            ExecuteAlias = ReactiveCommand.CreateFromTask<ExecutionContext, SearchContext>(OnExecuteAliasAsync, canExecuteAlias, outputScheduler: uiThread);
            ExecuteAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectNextResult = ReactiveCommand.Create(OnSelectNextResult, outputScheduler: uiThread);
            SelectNextResult.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectPreviousResult = ReactiveCommand.Create(OnSelectPreviousResult, outputScheduler: uiThread);
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
                .Where(x => IsSearchActivated)
                .Throttle(TimeSpan.FromMilliseconds(10), scheduler: uiThread)
                .Select(x => x.Trim())
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

        public ReactiveCommand<Unit, SearchContext> Activate { get; }

        public ReactiveCommand<Unit, string> AutoCompleteQuery { get; }

        [Reactive] public QueryResult CurrentAlias { get; set; }

        [Reactive] public int CurrentAliasIndex { get; set; }

        [Reactive] public string CurrentAliasSuggestion { get; set; }

        [Reactive] public string CurrentSessionName { get; set; }

        public ReactiveCommand<ExecutionContext, SearchContext> ExecuteAlias { get; }

        [Reactive] public bool IsOnError { get; set; }

        [Reactive] public bool IsSearchActivated { get; set; } = true;

        [Reactive] public bool KeepAlive { get; set; }

        [Reactive] public string Query { get; set; } = string.Empty;

        public IObservableCollection<QueryResult> Results { get; } = new ObservableCollectionExtended<QueryResult>();

        public ReactiveCommand<string, SearchContext> SearchAlias { get; }

        public ReactiveCommand<Unit, Unit> SelectNextResult { get; }

        public ReactiveCommand<Unit, Unit> SelectPreviousResult { get; }

        #endregion Properties

        #region Methods

        private SearchContext OnActivate()
        {
            return new()
            {
                CurrentSessionName = _service.GetDefaultSession()?.Name ?? "N.A."
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

            var cmd = _cmdlineManager.BuildFromText(context.Query);
            if (cmd.Parameters.IsNullOrWhiteSpace() && CurrentAlias is ExecutableQueryResult e)
            {
                cmd = _cmdlineManager.CloneWithNewParameters(e.Parameters, cmd);
            }

            IEnumerable<QueryResult> results = new List<QueryResult>();
            var scope = new Scope<bool>(x => IsSearchActivated = x, false, true);

            using (scope.Open())
            {
                if (CurrentAlias is null)
                {
                    _log.Warning("Current QueryResult is null");
                    results = DisplayQueryResult.SingleFromResult($"This alias does not exist");
                    KeepAlive = true;
                }
                else if (CurrentAlias is IExecutable exec)
                {
                    _log.Trace($"Found {results.Count()} element(s)");
                    Query = CurrentAlias is IQueryText t ? t.ToQuery() : CurrentAlias.ToQuery();


                    if (exec is IExecutableWithPrivilege exp)
                    {
                        exp.IsPrivilegeOverriden = context.RunAsAdmin;
                    }
                    results = await exec.ExecuteAsync(cmd);
                    KeepAlive = results.Any();
                }
                else
                {
                    _log.Info($"Alias '{CurrentAlias.Name}', is not executable. Add as a query");
                    KeepAlive = true;
                    results = Results;
                }
            }
            return new() { Results = results };
        }

        private SearchContext OnSearchAlias(string criterion)
        {
            if (criterion.IsNullOrWhiteSpace()) { return new(); }
            else
            {
                var query = _cmdlineManager.BuildFromText(criterion);
                var results = _searchService.Search(query);

                return new()
                {
                    Results = results,
                    CurrentAliasSuggestion = results.Any() && results.ElementAt(0).IsResult
                        ? results.ElementAt(0)?.Name ?? ""
                        : "",
                };
            }
        }

        private void OnSelectNextResult()
        {
            if (Results.CanNavigate())
            {
                CurrentAliasSuggestion = String.Empty;
                CurrentAliasIndex = Results.GetNextIndex(CurrentAliasIndex);
                _log.Trace($"Selecting next result. [Index: {CurrentAliasIndex}]");

                var scope = new Scope<bool>(x => IsSearchActivated = x, false, true);
                using (scope.Open())
                {
                    Query = CurrentAlias is IQueryText t ? t.ToQuery() : CurrentAlias.ToQuery();
                }
            }
        }

        private void OnSelectPreviousResult()
        {
            if (Results.CanNavigate())
            {
                CurrentAliasSuggestion = String.Empty;
                CurrentAliasIndex = Results.GetPreviousIndex(CurrentAliasIndex);
                _log.Trace($"Selecting previous result. [Index: {CurrentAliasIndex}]");

                var scope = new Scope<bool>(x => IsSearchActivated = x, false, true);
                using (scope.Open())
                {
                    Query = CurrentAlias is IQueryText t ? t.ToQuery() : CurrentAlias.ToQuery();
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