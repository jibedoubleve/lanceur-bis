using Dapper;
using FluentAssertions;
using Lanceur.Core.Models.Settings;
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
using NSubstitute;
using Serilog.Core;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class ApplicationSettingsViewModelShould : ViewModelTester<ApplicationSettingsViewModel>
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
                         .AddMockSingleton<IUserGlobalNotificationService>(
                             (sp, i) => visitors?.VisitGlobalUserInteractionService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserInteractionService>(
                             (sp, i) => visitors?.VisitUserInteractionService?.Invoke(sp, i) ?? i
                         )
                         .AddMockSingleton<IUserNotificationService>(
                             (sp, i) => visitors?.VisitUserNotificationService?.Invoke(sp, i) ?? i
                         )
                         .AddSingleton<IInteractionHubService, InteractionHubService>()
                         .AddSingleton(new LoggingLevelSwitch(LogEventLevel.Verbose));
        return serviceCollection;
    }

    [Fact]
    public void AskRebootAfterDbPathChanged()
    {
        IUserGlobalNotificationService userInteractionService = null;
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitGlobalUserInteractionService = (_, i) =>
            {
                userInteractionService = i;
                return i;
            }
        };
        TestViewModel(
            (viewModel, _) =>
            {
                // act
                viewModel.SaveSettingsCommand.Execute(null);

                viewModel.DbPath = Guid.NewGuid().ToString();
                viewModel.SaveSettingsCommand.Execute(null);

                // assert
                userInteractionService.Received(1)
                                      .AskRestart();
            },
            Sql.Empty,
            visitors
        );
    }

    [Fact]
    public void AskRebootAfterShortcutChanged()
    {
        IUserGlobalNotificationService userInteractionService = null;
        var visitors = new ServiceVisitors
        {
            OverridenConnectionString = ConnectionStringFactory.InMemory,
            VisitGlobalUserInteractionService = (_, i) =>
            {
                userInteractionService = i;
                return i;
            }
        };
        TestViewModel(
            (viewModel, _) =>
            {
                // arrange

                // act
                viewModel.Settings.Application.SetHotKey(3, 18); // This is the default configuration
                viewModel.SaveSettingsCommand.Execute(null);

                viewModel.Settings.Application.SetHotKey(1, 1);
                viewModel.SaveSettingsCommand.Execute(null);

                // assert
                userInteractionService.Received()
                                      .AskRestart();
            },
            Sql.Empty,
            visitors
        );
    }

    [Fact]
    public void SaveStoreShortcutOverrides()
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
        TestViewModel(
            (viewModel, db) =>
            {
                // arrange
                var sqlDefaultConfig = $"insert into settings(s_key, s_value) values ('json', '{defaultConfig}');";
                db.WithinTransaction(t => t.Connection!.Execute(sqlDefaultConfig));

                // act
                StoreShortcut[] shortcuts = [new() { AliasOverride = shortcut }, new()  { AliasOverride = shortcut }];
                viewModel.StoreShortcuts = new(shortcuts);
                viewModel.SaveSettingsCommand.Execute(null);

                // assert
                // I select all the shortcuts directly in the database
                const string sql = """
                                   select value ->> '$.AliasOverride' as AliasOverride
                                   from settings, json_each(json_extract(s_value, '$.Stores.StoreShortcuts'))
                                   where s_key = 'json';
                                   """;
                var results = db.WithConnection(c => c.Query<string>(sql))
                                .ToArray();

                results.Should().HaveCount(2);
                _ = results.Select(e => e.Should().Be(shortcut));
            },
            Sql.Empty,
            visitors
        );
    }

    #endregion
}