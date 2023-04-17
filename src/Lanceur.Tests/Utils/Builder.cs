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

        private ITestOutputHelper _output;
        private IScheduler _scheduler;

        #endregion Fields

        #region Constructors

        private Builder(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        public static Builder With(ITestOutputHelper output) => new(output);

        public MainViewModel BuildMainViewModel(ISearchService searchService = null, IExecutionManager executor = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(_scheduler));
            ArgumentNullException.ThrowIfNull(nameof(_output));

            Locator.CurrentMutable.UnregisterAll<ILogger>();
            var logger = new TestReactiveUiLogger(_output);
            Locator.CurrentMutable.RegisterConstant(logger, typeof(ILogger));

            return new MainViewModel(
                schedulerProvider: new TestSchedulerProvider(_scheduler),
                logFactory: new XUnitLoggerFactory(_output),
                searchService: searchService ?? Substitute.For<ISearchService>(),
                cmdlineService: new CmdlineManager(),
                executor: executor ?? Substitute.For<IExecutionManager>(),
                notify: Substitute.For<IUserNotification>()
            );
        }

        public Builder With(IScheduler scheduler)
        {
            _scheduler = scheduler;
            return this;
        }

        #endregion Methods
    }
}