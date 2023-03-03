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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class MainViewModel : ReactiveObject
    {
        #region Fields

        private readonly ICmdlineManager _cmdlineManager;
        private readonly IExecutionManager _executor;
        private readonly IAppLogger _log;
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
            IExecutionManager executor = null)
        {
            uiThread ??= RxApp.MainThreadScheduler;
            poolThread ??= RxApp.TaskpoolScheduler;

            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            _log = Locator.Current.GetLogger<MainViewModel>(logFactory);
            _searchService = searchService ?? l.GetService<ISearchService>();
            _cmdlineManager = cmdlineService ?? l.GetService<ICmdlineManager>();
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

            var isSearchinished = SearchAlias
                    .IsExecuting
                    .Select(x => !x);

            var canExecuteAlias = Observable.CombineLatest(
                isExecutable,
                isSearchinished
            ).Where(x => x.Where(y => !y).Any() == false)
             .Select(x => true); ;

            ExecuteAlias = ReactiveCommand.CreateFromTask<ExecutionRequest, AliasResponse>(OnExecuteAliasAsync, canExecuteAlias, outputScheduler: uiThread);
            ExecuteAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Observable.CombineLatest(
                this.WhenAnyObservable(vm => vm.ExecuteAlias.IsExecuting),
                this.WhenAnyObservable(vm => vm.SearchAlias.IsExecuting)
            ).DistinctUntilChanged()
             .Select(x => x.Where(x => x).Any())
             .BindTo(this, vm => vm.IsBusy);

            this.WhenAnyObservable(
                vm => vm.SelectNextResult,
                vm => vm.SelectPreviousResult
            ).BindTo(this, vm => vm.CurrentAlias);

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

            this.WhenAnyValue(vm => vm.Results)
                .Where(x => x is not null)
                .Select(x => x.FirstOrDefault())
                .BindTo(this, vm => vm.CurrentAlias);

            var obs = this.WhenAnyObservable(vm => vm.SearchAlias, vm => vm.ExecuteAlias);
            obs.Select(r => r.Results.ToObservableCollection())
                .BindTo(this, vm => vm.Results);

            obs.Select(r => r.KeepAlive)
                .BindTo(this, vm => vm.KeepAlive);

            this.WhenAnyObservable(vm => vm.Activate)
                .Subscribe(x => CurrentSessionName = x.CurrentSessionName);
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, AliasResponse> Activate { get; }

        public ReactiveCommand<Unit, string> AutoComplete { get; }

        [Reactive] public QueryResult CurrentAlias { get; set; }

        [Reactive] public string CurrentSessionName { get; set; }

        public ReactiveCommand<ExecutionRequest, AliasResponse> ExecuteAlias { get; }

        [Reactive] public bool IsBusy { get; set; }

        [Reactive] public bool IsOnError { get; set; }

        [Reactive] public bool KeepAlive { get; set; }

        [Reactive] public string Query { get; set; } = string.Empty;

        [Reactive]
        public ObservableCollection<QueryResult> Results { get; set; } = new();

        public ReactiveCommand<string, AliasResponse> SearchAlias { get; }

        public ReactiveCommand<Unit, QueryResult> SelectNextResult { get; }

        public ReactiveCommand<Unit, QueryResult> SelectPreviousResult { get; }

        #endregion Properties

        #region Methods

        private async Task<AliasResponse> OnActivate()
        {
            var sessionName = await Task.Run(() => _service.GetDefaultSession()?.Name ?? "N.A.");
            return new() { CurrentSessionName = sessionName, };
        }

        private string OnAutoComplete() => CurrentAlias?.Name ?? string.Empty;

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
                    KeepAlive = results.Any()
                };
            }
        }

        #endregion Methods

        #region Classes

        public class AliasResponse
        {
            #region Properties

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

            public static implicit operator ExecutionRequest(string query) => new() { Query = query };

            #endregion Methods
        }

        #endregion Classes
    }
}