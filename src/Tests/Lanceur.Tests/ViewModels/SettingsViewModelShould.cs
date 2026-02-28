using Dapper;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public class SettingsViewModelShould : ViewModelTester<ApplicationSettingsViewModel>
{
    #region Constructors

    public SettingsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private void AssertProperty(Action<ApplicationSettingsViewModel> act, Func<string> getSql, Func<string> expected)
    {
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };

        TestViewModel(
            (viewModel, db) =>
            {
                // arrange
                // The data is copied in the DB the first time the
                // values are updated. Initial save will enure there's
                // data to compare.
                viewModel.SaveSettings();

                // act
                act(viewModel);

                // assert
                var sql = getSql();
                var result = db.WithConnection(c => c.Query<string>(sql).SingleOrDefault());
                result.ShouldBe(expected());
            },
            Sql.Empty,
            visitors
        );
    }

    private static string GetProperty(string jsonPath)
        => $"""
            select s_value ->> '$.{jsonPath}' as value
            from settings
            where s_key = 'json'
            """;

    protected override IServiceCollection ConfigureServices(
        IServiceCollection serviceCollection,
        ServiceVisitors visitors
    )
    {
        serviceCollection.AddTestOutputHelper(OutputHelper)
                         .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose))
                         .AddSingleton<IConfigurationFacade, ConfigurationFacadeService>()
                         .AddSingleton<IUserCommunicationService, UserCommunicationService>()
                         .AddSingleton<IInfrastructureSettingsProvider, MemoryInfrastructureSettingsProvider>()
                         .AddSingleton<IApplicationSettingsProvider, SQLiteApplicationSettingsProvider>()
                         .AddMockSingleton<IUserGlobalNotificationService>()
                         .AddMockSingleton<IUserNotificationService>()
                         .AddMockSingleton<IAppRestartService>()
                         .AddMockSingleton<IViewFactory>()
                         .AddMockSingleton<IUserDialogueService>();
        return serviceCollection;
    }

    [Theory]
    [InlineData(500)]
    [InlineData(123)]
    public void SaveOptionSearchDelay(double value)
        => AssertProperty(
            viewModel => viewModel.SearchDelay = value,
            () => GetProperty("SearchBox.SearchDelay"),
            value.ToString
        );

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveOptionShowAtStartup(bool value)
        => AssertProperty(
            viewModel => viewModel.ShowAtStartup = value,
            () => GetProperty("SearchBox.ShowAtStartup"),
            () => Convert.ToInt32(value).ToString()
        );

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveOptionShowLastQuery(bool value)
        => AssertProperty(
            viewModel => viewModel.ShowLastQuery = value,
            () => GetProperty("SearchBox.ShowLastQuery"),
            () => Convert.ToInt32(value).ToString()
        );

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SaveOptionShowResult(bool value)
        =>  AssertProperty(
            viewModel => viewModel.ShowResult = value,
            () => GetProperty("SearchBox.ShowResult"),
            () => Convert.ToInt32(value).ToString()
        );

    [Theory]
    [InlineData("Zen")]
    [InlineData("Chrome")]
    [InlineData("Edge")]
    [InlineData("Firefox")]
    [InlineData("SomeUnknownValue")]
    public void SaveOptionStoresBookmarkSourceBrowser(string value)
        => AssertProperty(
            viewModel => viewModel.BookmarkSourceBrowser = value,
            () => GetProperty("Stores.BookmarkSourceBrowser"),
            () => value
        );

    #endregion
}