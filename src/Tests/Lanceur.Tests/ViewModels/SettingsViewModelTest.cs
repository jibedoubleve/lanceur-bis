using Dapper;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Fakes;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.ViewModels;

public sealed class SettingsViewModelTest : ViewModelTester<ApplicationSettingsViewModel>
{
    #region Constructors

    public SettingsViewModelTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private void AssertProperty(Action<ApplicationSettingsViewModel> act, Func<string> getSql, Func<string> expected)
    {
        var hooks = new ServiceHooks { OverridenConnectionString = ConnectionStringFactory.InMemory };

        TestViewModel(
            (viewModel, db) => {
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
            hooks
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
        ServiceHooks? hooks
    )
    {
        serviceCollection.AddLoggingForTests(OutputHelper)
                         .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose))
                         .AddSingleton<ISettingsProvider<InfrastructureSettings>,
                             MemoryInfrastructureSettingsProvider>()
                         .AddSingleton<ISettingsProvider<ApplicationSettings>, SQLiteApplicationSettingsProvider>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>()
                         .AddUiServicesMockContext();
        return serviceCollection;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_ShowAtStartup_changes_Then_it_is_saved_in_db(bool value)
        => AssertProperty(
            viewModel => viewModel.ShowAtStartup = value,
            () => GetProperty("SearchBox.ShowAtStartup"),
            () => Convert.ToInt32(value).ToString()
        );

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_ShowLastQuery_changes_Then_it_is_saved_in_db(bool value)
        => AssertProperty(
            viewModel => viewModel.ShowLastQuery = value,
            () => GetProperty("SearchBox.ShowLastQuery"),
            () => Convert.ToInt32(value).ToString()
        );

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_ShowResult_changes_Then_it_is_saved_in_db(bool value)
        => AssertProperty(
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
    public void When_bookmark_source_browser_changes_Then_it_is_saved_in_db(string value)
        => AssertProperty(
            viewModel => viewModel.BookmarkSourceBrowser = value,
            () => GetProperty("Stores.BookmarkSourceBrowser"),
            () => value
        );

    [Theory]
    [InlineData(500)]
    [InlineData(123)]
    public void When_search_delay_changes_Then_it_is_saved_in_db(double value)
        => AssertProperty(
            viewModel => viewModel.SearchDelay = value,
            () => GetProperty("SearchBox.SearchDelay"),
            value.ToString
        );

    #endregion
}