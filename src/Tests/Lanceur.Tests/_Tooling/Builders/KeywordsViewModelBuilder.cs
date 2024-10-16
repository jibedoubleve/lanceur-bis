using System.Reactive.Concurrency;
using Lanceur.Core.Managers;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Ui;
using Lanceur.Views;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling.Builders;

internal class KeywordsViewModelBuilder
{
    #region Fields

    private IDbRepository _dbRepository;
    private ILoggerFactory _loggerFactory;
    private IPackagedAppSearchService _packagedAppSearchService;
    private TestSchedulerProvider _schedulerProvider;

    #endregion Fields

    #region Methods

    public KeywordsViewModel Build() => new(
        _loggerFactory ?? throw new ArgumentNullException(nameof(_loggerFactory), "Log factory is mandatory"),
        _dbRepository ?? Substitute.For<IDbRepository>(),
        packagedAppSearchService: _packagedAppSearchService ?? Substitute.For<IPackagedAppSearchService>(),
        schedulers: _schedulerProvider ?? throw new ArgumentNullException($"No scheduler configured for the ViewModel to test."),
        notify: Substitute.For<IUiNotification>(),
        thumbnailManager: Substitute.For<IThumbnailManager>(),
        notification: Substitute.For<INotification>()
    );

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
        _loggerFactory = new MicrosoftLoggingLoggerFactory(output);
        return this;
    }

    public KeywordsViewModelBuilder With(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    #endregion Methods
}