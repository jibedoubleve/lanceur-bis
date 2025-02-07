using Dapper;
using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.ViewModels;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class ApplicationSettingsViewModelShould : ViewModelTest<ApplicationSettingsViewModel>
{
    #region Constructors

    public ApplicationSettingsViewModelShould(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    protected override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, ServiceVisitors visitors)
    {
        serviceCollection.AddMockSingleton<IAppRestartService>()
                         .AddSingleton<ISettingsFacade, SettingsFacadeService>()
                         .AddSingleton<IDatabaseConfigurationService, SQLiteDatabaseConfigurationService>()
                         .AddSingleton<IApplicationConfigurationService, MemoryApplicationConfigurationService>()
                         .AddMockSingleton<IViewFactory>()
                         .AddMockSingleton<IUserInteractionService>(
                             (sp, i) => visitors?.VisitUserInteractionService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserNotificationService>(
                             (sp, i) => visitors?.VisitUserNotificationService?.Invoke(sp, i) ?? i
                         )
                         .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose));
        return serviceCollection;
    }

    [Fact]
    public async Task SaveStoreShortcutOverrides()
    {
        const string shortcut = "shortcut";
        const string defaultConfig = """
                                     {
                                         "HotKey": {
                                             "Key": 18,
                                             "ModifierKey": 3
                                         },
                                         "SearchBox": {
                                             "SearchDelay": 50.0,
                                             "ShowAtStartup": true,
                                             "ShowLastQuery": true,
                                             "ShowResult": false
                                         },
                                         "Stores": {
                                             "BookmarkSourceBrowser": "Zen",
                                             "StoreOverrides": [
                                                 {
                                                     "AliasOverride": "^\\s{0,}/.*",
                                                     "StoreType": "Lanceur.Infra.Stores.BookmarksStore"
                                                 },
                                                 {
                                                     "AliasOverride": "^\\s{0,}::.*",
                                                     "StoreType": "Lanceur.Infra.Stores.EverythingStore"
                                                 }
                                             ]
                                         },
                                         "Window": {
                                             "Position": {
                                                 "Left": 2200.0,
                                                 "Top": 200.0
                                             },
                                             "BackdropStyle": "Acrylic"
                                         }
                                     }
                                     """;
        var visitors = new ServiceVisitors { OverridenConnectionString = ConnectionStringFactory.InMemory };
        await TestViewModel(
            async (viewModel, db) =>
            {
                // arrange
                var sqlDefaultConfig = $"insert into settings(s_key, s_value) values ('json', '{defaultConfig}');";
                db.WithinTransaction(t => t.Connection!.Execute(sqlDefaultConfig));
                
                // act
                StoreShortcut[] shortcuts = [new() { AliasOverride = shortcut }, new()  { AliasOverride = shortcut }];
                viewModel.StoreShortcuts = new (shortcuts);
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);

                // assert
                // I select all the shortcuts directly in the database
                const string sql = """
                                   select value ->> '$.AliasOverride' as AliasOverride
                                   from settings, json_each(json_extract(s_value, '$.Stores.StoreOverrides'))
                                   where s_key = 'json';
                                   """;
                var results = db.WithConnection(c => c.Query<string>(sql))
                                .ToArray();
                
                results.Should().HaveCount(2);
                _ = results.Select(e => e.Should().Be(shortcut));

            },
            SqlBuilder.Empty,
            visitors
        );
    }

    [Fact]
    public async Task AskRebootAfterShortcutChanged()
    {
        IUserInteractionService userInteractionService = null;
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                userInteractionService = i;
                return i;
            }
        };
        await TestViewModel(
            async (viewModel, _) =>
            {
                // arrange
                
                // act
                viewModel.Settings.Application.HotKey = new(3, 18); // This is the default configuration
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);
                
                viewModel.Settings.Application.HotKey = new(1, 1);
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);

                // assert
                await userInteractionService.Received(1)
                                            .AskUserYesNoAsync(Arg.Any<object>());
            },
            SqlBuilder.Empty,
            visitors
        );
    }
    
    [Fact]
    public async Task AskRebootAfterDbPathChanged()
    {
        IUserInteractionService userInteractionService = null;
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitUserInteractionService = (_, i) =>
            {
                userInteractionService = i;
                return i;
            }
        };
        await TestViewModel(
            async (viewModel, _) =>
            {
                // arrange
                
                // act
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);
                
                viewModel.DbPath = Guid.NewGuid().ToString();
                await viewModel.SaveSettingsCommand.ExecuteAsync(null);

                // assert
                await userInteractionService.Received(1)
                                            .AskUserYesNoAsync(Arg.Any<object>());
            },
            SqlBuilder.Empty,
            visitors
        );
    }

    #endregion
}