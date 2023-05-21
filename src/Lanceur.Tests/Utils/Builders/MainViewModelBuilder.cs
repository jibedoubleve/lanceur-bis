using Lanceur.Core.Managers;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.Core.Services.Config;
using Lanceur.Infra.Managers;
using Lanceur.Schedulers;
using Lanceur.Tests.Logging;
using Lanceur.Views;
using NSubstitute;
using System.Reactive.Concurrency;
using Xunit.Abstractions;

namespace Lanceur.Tests.Utils
{
    internal class MainViewModelBuilder
    {
        #region Fields

        private IAppConfigService _appConfigService;
        private IExecutionManager _executionManager;
        private ITestOutputHelper _output;
        private ISchedulerProvider _schedulerProvider;
        private ISearchService _searchService;
        private IDataService _dataService;

        #endregion Fields

        #region Methods

        internal MainViewModelBuilder With(IAppConfigService appConfigService)
        {
            _appConfigService = appConfigService;
            return this;
        }

        public MainViewModel Build()
        {
            ArgumentNullException.ThrowIfNull(_output);
            ArgumentNullException.ThrowIfNull(_schedulerProvider);

            var appConfigService = Substitute.For<IAppConfigService>();
            appConfigService.Current.Returns(new AppConfig());

            var dataService = Substitute.For<IDataService>();

            return new MainViewModel(
                schedulerProvider: _schedulerProvider,
                logFactory: new XUnitLoggerFactory(_output),
                searchService: _searchService ?? Substitute.For<ISearchService>(),
                cmdlineService: new CmdlineManager(),
                executor: _executionManager ?? Substitute.For<IExecutionManager>(),
                notify: Substitute.For<IUserNotification>(),
                appConfigService: _appConfigService ?? appConfigService,
                dataService: _dataService ?? dataService
            ) ;
        }

        public MainViewModelBuilder With(ITestOutputHelper output)
        {
            _output = output;
            return this;
        }

        public MainViewModelBuilder With(ISearchService searchService)
        {
            _searchService = searchService;
            return this;
        }

        public MainViewModelBuilder With(IExecutionManager executionManager)
        {
            _executionManager = executionManager;
            return this;
        }

        public MainViewModelBuilder With(IScheduler scheduler)
        {
            _schedulerProvider = new TestSchedulerProvider(scheduler);
            return this;
        }

        public MainViewModelBuilder With(IDataService dataService)
        {
            _dataService = dataService;
            return this;
        }

        #endregion Methods
    }
}