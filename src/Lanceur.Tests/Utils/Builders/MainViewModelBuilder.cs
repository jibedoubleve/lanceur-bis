using Lanceur.Core.Managers;
using Lanceur.Core.Services;
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

        private IExecutionManager _executionManager;
        private ITestOutputHelper _output;
        private ISchedulerProvider _schedulerProvider;
        private ISearchService _searchService;

        #endregion Fields

        #region Methods

        public MainViewModel Build()
        {
            ArgumentNullException.ThrowIfNull(nameof(_output));

            ArgumentNullException.ThrowIfNull(nameof(_schedulerProvider));

            return new MainViewModel(
                schedulerProvider: _schedulerProvider,
                logFactory: new XUnitLoggerFactory(_output),
                searchService: _searchService ?? Substitute.For<ISearchService>(),
                cmdlineService: new CmdlineManager(),
                executor: _executionManager ?? Substitute.For<IExecutionManager>(),
                notify: Substitute.For<IUserNotification>()
            );
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

        #endregion Methods
    }
}