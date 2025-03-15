using Dapper;
using FluentAssertions;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class SettingsViewModelShould : ViewModelTest<ApplicationSettingsViewModel>
{
    #region Constructors

    public SettingsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    private async Task CheckProperty(Action<ApplicationSettingsViewModel> act, Func<string> getSql, Func<string> expected)
    {
        var sqlBuilder = new SqlBuilder();
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };

        await TestViewModelAsync(
            async (viewModel, db) =>
            {
                // act
                act(viewModel);
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);

                // assert
                var sql = getSql();
                var result = db.WithConnection(c => c.Query<string>(sql).Single());
                result.Should().Be(expected());
            },
            sqlBuilder,
            visitors
        );
    }

    private string Sql(string jsonQuery) => $"""
                                             select s_value ->> '$.{jsonQuery}' as value
                                             from settings
                                             where s_key = 'json'
                                             """;

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddLogger<ApplicationSettingsViewModel>(OutputHelper)
                         .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose))
                         .AddSingleton<ISettingsFacade, SettingsFacadeService>()
                         .AddSingleton<IApplicationConfigurationService, MemoryApplicationConfigurationService>()
                         .AddSingleton<IDatabaseConfigurationService, SQLiteDatabaseConfigurationService>()
                         .AddMockSingleton<IUserNotificationService>()
                         .AddMockSingleton<IAppRestartService>()
                         .AddMockSingleton<IViewFactory>()
                         .AddMockSingleton<IUserInteractionService>();
        return serviceCollection;
    }

    [Theory]
    [InlineData(500, "500")]
    public async Task SaveOptionSearchDelay(double value, string expected) => await CheckProperty(
        viewModel => viewModel.SearchDelay = value,
        () => Sql("SearchBox.SearchDelay"),
        () => expected
    );

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public async Task SaveOptionShowAtStartup(bool value, string expected) => await CheckProperty(
        viewModel => viewModel.ShowAtStartup = value,
        () => Sql("SearchBox.ShowAtStartup"),
        () => expected
    );

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public async Task SaveOptionShowLastQuery(bool value, string expected) => await CheckProperty(
        viewModel => viewModel.ShowLastQuery = value,
        () => Sql("SearchBox.ShowLastQuery"),
        () => expected
    );

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public async Task SaveOptionShowResult(bool value, string expected) => await CheckProperty(
        viewModel => viewModel.ShowResult = value,
        () => Sql("SearchBox.ShowResult"),
        () => expected
    );

    [Theory]
    [InlineData("Zen")]
    [InlineData("Chrome")]
    [InlineData("Edge")]
    [InlineData("Firefox")]
    [InlineData("SomeUnknownValue")]
    public async Task SaveOptionStoresBookmarkSourceBrowser(string value) => await CheckProperty(
        viewModel => viewModel.BookmarkSourceBrowser = value,
        () => Sql("Stores.BookmarkSourceBrowser"),
        () => value
    );

    #endregion
}