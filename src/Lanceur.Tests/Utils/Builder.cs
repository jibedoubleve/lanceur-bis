using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Schedulers;
using Lanceur.Tests.Logging;
using Lanceur.Views;
using NSubstitute;
using Splat;
using System.Reactive.Concurrency;
using Xunit.Abstractions;

namespace Lanceur.Tests.Utils
{
    internal class Builder
    {
        #region Fields

        private IExecutionManager _executionManager;
        private ITestOutputHelper _output;
        private ISchedulerProvider _schedulerProvider;
        private ISearchService _searchService;
        private bool _useLocator;

        #endregion Fields

        #region Constructors

        private Builder(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        public static Builder Build(ITestOutputHelper output) => new(output);

        public MainViewModel BuildMainViewModel()
        {
            ArgumentNullException.ThrowIfNull(nameof(_output));

            if (!_useLocator) { ArgumentNullException.ThrowIfNull(nameof(_schedulerProvider)); }
            else { _schedulerProvider = Locator.Current.GetService<ISchedulerProvider>(); }

            Locator.CurrentMutable.UnregisterAll<ILogger>();
            var logger = new TestReactiveUiLogger(_output);
            Locator.CurrentMutable.RegisterConstant(logger, typeof(ILogger));

            return new MainViewModel(
                schedulerProvider: _schedulerProvider,
                logFactory: new XUnitLoggerFactory(_output),
                searchService: _searchService ?? Substitute.For<ISearchService>(),
                cmdlineService: new CmdlineManager(),
                executor: _executionManager ?? Substitute.For<IExecutionManager>(),
                notify: Substitute.For<IUserNotification>()
            );
        }

        public Builder UseLocator()
        {
            _useLocator = true;
            return this;
        }

        public Builder With(ISearchService searchService)
        {
            _searchService = searchService;
            return this;
        }

        public Builder With(IExecutionManager executionManager)
        {
            _executionManager = executionManager;
            return this;
        }

        public Builder With(IScheduler scheduler)
        {
            _schedulerProvider = new TestSchedulerProvider(scheduler);
            return this;
        }

        #endregion Methods
    }
}