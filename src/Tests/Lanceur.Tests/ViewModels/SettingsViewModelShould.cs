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
using Lanceur.Ui.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

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
                // act
                viewModel.SaveSettingsCommand.Execute(null); //Force default values
                act(viewModel);

                // assert
                var sql = getSql();
                var result = db.WithConnection(c => c.Query<string>(sql).SingleOrDefault());
                result.Should().Be(expected());
            },
            SqlBuilder.Empty,
            visitors
        );
    }

    private static string GetProperty(string jsonPath) => $"""
                                                           select s_value ->> '$.{jsonPath}' as value
                                                           from settings
                                                           where s_key = 'json'
                                                           """;

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddLoggingForTests<ApplicationSettingsViewModel>(OutputHelper)
                         .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose))
                         .AddSingleton<ISettingsFacade, SettingsFacadeService>()
                         .AddSingleton<IInteractionHub, InteractionHub>()
                         .AddSingleton<IApplicationConfigurationService, MemoryApplicationConfigurationService>()
                         .AddSingleton<IDatabaseConfigurationService, SQLiteDatabaseConfigurationService>()
                         .AddMockSingleton<IUserGlobalNotificationService>()
                         .AddMockSingleton<IUserNotificationService>()
                         .AddMockSingleton<IAppRestartService>()
                         .AddMockSingleton<IViewFactory>()
                         .AddMockSingleton<IUserInteractionService>();
        return serviceCollection;
    }

    [Theory]
    [InlineData(500, "500")]
    [InlineData(123, "123")]
    public void SaveOptionSearchDelay(double value, string expected) => AssertProperty(
        viewModel => viewModel.SearchDelay = value,
        () => GetProperty("SearchBox.SearchDelay"),
        () => expected
    );

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public void SaveOptionShowAtStartup(bool value, string expected) => AssertProperty(
        viewModel => viewModel.ShowAtStartup = value,
        () => GetProperty("SearchBox.ShowAtStartup"),
        () => expected
    );

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public void SaveOptionShowLastQuery(bool value, string expected) => AssertProperty(
        viewModel => viewModel.ShowLastQuery = value,
        () => GetProperty("SearchBox.ShowLastQuery"),
        () => expected
    );

    [Theory]
    [InlineData(true, "1")]
    [InlineData(false, "0")]
    public void SaveOptionShowResult(bool value, string expected) =>  AssertProperty(
        viewModel => viewModel.ShowResult = value,
        () => GetProperty("SearchBox.ShowResult"),
        () => expected
    );

    [Theory]
    [InlineData("Zen")]
    [InlineData("Chrome")]
    [InlineData("Edge")]
    [InlineData("Firefox")]
    [InlineData("SomeUnknownValue")]
    public void SaveOptionStoresBookmarkSourceBrowser(string value) => AssertProperty(
        viewModel => viewModel.BookmarkSourceBrowser = value,
        () => GetProperty("Stores.BookmarkSourceBrowser"),
        () => value
    );

    #endregion
}