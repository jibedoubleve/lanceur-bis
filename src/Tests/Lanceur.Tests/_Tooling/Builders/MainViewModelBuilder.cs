using System.Reactive.Concurrency;
using Lanceur.Core.Managers;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Schedulers;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Views;
using NSubstitute;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling.Builders;

internal class MainViewModelBuilder
{
    #region Fields

    private ISettingsFacade _appConfigService;
    private IExecutionManager _executionManager;
    private ITestOutputHelper _output;
    private ISchedulerProvider _schedulerProvider;
    private ISearchService _searchService;

    #endregion Fields

    #region Methods

    public MainViewModel Build()
    {
        ArgumentNullException.ThrowIfNull(_output);
        ArgumentNullException.ThrowIfNull(_schedulerProvider);

        var settingsFacade = Substitute.For<ISettingsFacade>();
        settingsFacade.Application.Returns(new AppConfig());

        return new(
            _schedulerProvider ?? throw new ArgumentNullException($"No scheduler configured for the ViewModel to test."),
            searchService: _searchService ?? Substitute.For<ISearchService>(),
            executor: _executionManager ?? Substitute.For<IExecutionManager>(),
            notify: Substitute.For<IUserNotification>(),
            appConfigService: _appConfigService ?? settingsFacade,
            dataService: Substitute.For<IDbRepository>(),
            loggerFactory: new MicrosoftLoggingLoggerFactory(_output)
        );
    }

    public MainViewModelBuilder With(ISettingsFacade settingsFacade)
    {
        _appConfigService = settingsFacade;
        return this;
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