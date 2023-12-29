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
    private TestSchedulerProvider _schedulerProvider;
    private IPackagedAppSearchService _packagedAppSearchService;

    #endregion Fields

    #region Methods

    public KeywordsViewModel Build()
    {
        return new(
            logFactory: _loggerFactory ?? throw new ArgumentNullException(nameof(_loggerFactory), "Log factory is mandatory"),
            searchService: _dbRepository ?? Substitute.For<IDbRepository>(),
            packagedAppSearchService: _packagedAppSearchService ?? Substitute.For<IPackagedAppSearchService>(),
            schedulers: _schedulerProvider ?? throw new ArgumentNullException($"No scheduler configured for the ViewModel to test."),
            notify: Substitute.For<IUserNotification>(),
            thumbnailManager: Substitute.For<IThumbnailManager>(),
            notification: Substitute.For<INotification>()
        );
    }

    public KeywordsViewModelBuilder With(IDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
        return this;
    }

    public KeywordsViewModelBuilder With(IScheduler scheduler)
    {
        _schedulerProvider = new(scheduler);
        return this;
    }

    public KeywordsViewModelBuilder With(IPackagedAppSearchService packagedAppSearchService)
    {
        _packagedAppSearchService = packagedAppSearchService;
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