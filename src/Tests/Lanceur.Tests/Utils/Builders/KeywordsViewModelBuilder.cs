using Lanceur.Core.Managers;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Tests.Logging;
using Lanceur.Ui;
using Lanceur.Views;
using NSubstitute;
using System.Reactive.Concurrency;
using Xunit.Abstractions;

namespace Lanceur.Tests.Utils.Builders;

internal class KeywordsViewModelBuilder
{
    #region Fields

    private IDbRepository _dbRepository;
    private IAppLoggerFactory _loggerFactory;
    private IFavIconManager _favIconManager;
    private TestSchedulerProvider _schedulerProvider;

    #endregion Fields

    #region Methods

    public KeywordsViewModel Build()
    {
        return new(
            logFactory: _loggerFactory,
            searchService: _dbRepository ?? Substitute.For<IDbRepository>(),
            schedulers: _schedulerProvider ?? throw new ArgumentNullException($"No scheduler configured for the ViewModel to test."),
            notify: Substitute.For<IUserNotification>(),
            thumbnailManager: Substitute.For<IThumbnailManager>(),
            notification: Substitute.For<INotification>()
        );
    }

    public KeywordsViewModelBuilder With(IFavIconManager favIconManager)
    {
        _favIconManager = favIconManager;
        return this;
    }

    public KeywordsViewModelBuilder With(IDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
        return this;
    }

    public KeywordsViewModelBuilder With(IScheduler scheduler)
    {
        _schedulerProvider = new TestSchedulerProvider(scheduler);
        return this;
    }

    public KeywordsViewModelBuilder With(ITestOutputHelper output)
    {
        _loggerFactory = new XUnitLoggerFactory(output);
        return this;
    }

    public KeywordsViewModelBuilder With(IAppLoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    #endregion Methods
}