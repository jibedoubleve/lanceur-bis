using System.Data;
using System.Reflection;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Macros;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Stores;
using Lanceur.Infra.Utils;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Tests.Tools.Macros;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using MacroAliasExpanderService = Lanceur.Infra.Services.MacroAliasExpanderService;

namespace Lanceur.Tests.Services;

public class MacroServiceTest : TestBase
{
    #region Constructors

    public MacroServiceTest(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Fact]
    public void When_expanding_Multi_macro_Then_it_is_SelfExecutableQueryResult()
    {
        var serviceProvider = new ServiceCollection().AddMockSingleton<IAliasRepository>()
                                                     .AddMockSingleton<ILogger<MacroAliasExpanderService>>()
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddSingleton(
                                                         new AssemblySource
                                                         {
                                                             MacroSource = Assembly.GetExecutingAssembly()
                                                         }
                                                     )
                                                     .AddSingleton<MacroAliasExpanderService>()
                                                     .AddSingleton<MultiMacroTest>()
                                                     .BuildServiceProvider();
        var macroMgr = serviceProvider.GetService<MacroAliasExpanderService>();
        var macro = new MultiMacroTest();
        var result = macroMgr.Expand(macro).First();

        result.ShouldBeAssignableTo<SelfExecutableQueryResult>();
    }

    [Theory]
    [InlineData("init", "a@z@e@r@t@t")]
    [InlineData("home", "azerty")]
    [InlineData("some", "a z e r t y")]
    public async Task When_handling_macro_Then_name_and_description_are_not_modified(string name, string parameters)
    {
        var serviceProvider = new ServiceCollection().AddMockSingleton<ILogger<MacroAliasExpanderService>>()
                                                     .AddMockSingleton<IAliasRepository>()
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddSingleton(
                                                         new AssemblySource
                                                         {
                                                             MacroSource = Assembly.GetExecutingAssembly()
                                                         }
                                                     )
                                                     .AddSingleton<MacroAliasExpanderService>()
                                                     .BuildServiceProvider();

        var expander = serviceProvider.GetService<MacroAliasExpanderService>();
        var macro = new MultiMacroTest { Parameters = parameters };
        var handler = (SelfExecutableQueryResult)expander.Expand(macro).First();

        var cmdline = new Cmdline(name, parameters);
        var results = (await handler.ExecuteAsync(cmdline)).ToArray();

        results.ShouldSatisfyAllConditions(
            r => r[0].Name.ShouldBe(name),
            r => r[0].Description.ShouldBe(parameters)
        );
    }

    private ServiceProvider BuildMacroServiceProvider()
    {
        
        var asm = new AssemblySource { MacroSource = Assembly.GetAssembly(typeof(MultiMacro)) };
        return new ServiceCollection().AddLoggerFactoryForTests(OutputHelper)
                            .AddLogging()
                            .AddMockSingleton<IAliasRepository>()
                            .AddLoggerFactoryForTests(OutputHelper)
                            .AddMockSingleton<IExecutionService>()
                            .AddMockSingleton<ISearchService>()
                            .AddSingleton(
                                new AssemblySource
                                {
                                    MacroSource = Assembly.GetAssembly(typeof(MultiMacro))
                                }
                            )
                            .AddSingleton<MacroAliasExpanderService>()
                            .AddSingleton(_ => asm)
                            .AddMockSingleton<IClipboardService>()
                            .AddMockSingleton<IGithubService>()
                            .AddMockSingleton<IUserGlobalNotificationService>()
                            .AddMockSingleton<IEnigma>()
                            .AddMockSingleton<ISection<GithubSection>>()
                            .AddMacroServices()
                            .BuildServiceProvider();
    }
    [Fact]
    public void When_loading_macro_Then_id_and_count_are_not_modified()
    {
        // ARRANGE
        const long expectedId = 952;
        const int expectedCount = 259;
        QueryResult[] queryResults =
        [
            new AliasQueryResult
            {
                Name = "macro_1",
                FileName = "@multi@",
                Id = expectedId,
                Count = expectedCount
            }
        ];

        var serviceProvider = BuildMacroServiceProvider();

        // ACT
        var output = serviceProvider.GetService<MacroAliasExpanderService>()
                                    .Expand(queryResults)
                                    .ToArray();
        // ASSERT
        output.Length.ShouldBe(1);

        output[0]
            .ShouldSatisfyAllConditions(
                m => m.Id.ShouldBe(expectedId),
                m => m.Count.ShouldBe(expectedCount)
            );
    }

    [Fact]
    public void When_loading_macros_Then_at_least_one_macro_should_exist_by_default()
    {
        var serviceProvider = new ServiceCollection()
                              .AddSingleton(
                                  new AssemblySource { MacroSource = Assembly.GetAssembly(typeof(MultiMacro))! }
                              )
                              .AddSingleton<ILoggerFactory, LoggerFactory>()
                              .AddMockSingleton<ILogger<MacroAliasExpanderService>>()
                              .AddMockSingleton<IExecutionService>()
                              .AddMockSingleton<ISearchService>()
                              .AddMockSingleton<IAliasRepository>()
                              .AddSingleton<MacroAliasExpanderService>()
                              .AddMockSingleton<IClipboardService>()
                              .AddLoggerFactoryForTests(OutputHelper)
                              .AddLogging()
                              .AddMockSingleton<IGithubService>()
                              .AddMockSingleton<IUserGlobalNotificationService>()
                              .AddMockSingleton<IEnigma>()
                              .AddMockSingleton<ISection<GithubSection>>()
                              .AddMacroServices()
                              .BuildServiceProvider();

        var manager = serviceProvider.GetService<MacroAliasExpanderService>();
        manager.MacroCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void When_loading_macros_Then_expected_amount_of_macro_found()
    {
        var serviceProvider = BuildMacroServiceProvider();
        var macroService = serviceProvider.GetService<MacroAliasExpanderService>();
        macroService.MacroCount.ShouldBe(4);
    }

    [Fact]
    public void When_macro_is_called_multiple_times_Then_no_doubloons_is_created()
    {
        QueryResult[] queryResults =
        [
            new AliasQueryResult { Name = "macro_1", FileName = "@multi@" },
            new AliasQueryResult { Name = "macro_2", FileName = "@multi@" },
            new AliasQueryResult { Name = "macro_3", FileName = "@multi@" }
        ];


        var asm = new AssemblySource { MacroSource = Assembly.GetAssembly(typeof(MultiMacro)) };
        var serviceProvider = new ServiceCollection().AddMockSingleton<ILogger<MacroAliasExpanderService>>()
                                                     .AddMockSingleton<IAliasRepository>()
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddMockSingleton<IExecutionService>()
                                                     .AddMockSingleton<ISearchService>()
                                                     .AddSingleton(
                                                         new AssemblySource
                                                         {
                                                             MacroSource = Assembly.GetAssembly(typeof(MultiMacro))
                                                         }
                                                     )
                                                     .AddSingleton<MacroAliasExpanderService>()
                                                     .AddSingleton(_ => asm)
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddMockSingleton<IClipboardService>()
                                                     .AddSingleton(_ => asm)
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddLogging()
                                                     .AddMockSingleton<IGithubService>()
                                                     .AddMockSingleton<IUserGlobalNotificationService>()
                                                     .AddMockSingleton<IEnigma>()
                                                     .AddMockSingleton<ISection<GithubSection>>()
                                                     .AddMacroServices()
                                                     .BuildServiceProvider();

        var output = serviceProvider.GetService<MacroAliasExpanderService>()
                                    .Expand(queryResults)
                                    .ToArray();

        output.ShouldSatisfyAllConditions(
            o => o.GetDoubloons().Count().ShouldBe(0),
            o => o.Length.ShouldBe(3)
        );
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    public void When_retrieving_multi_macro_Then_it_has_expected_delays(int index, int delay)
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db, OutputHelper);

        // Act
        var results = service.Search("alias1");

        // Assert
        var composite = results.ElementAt(0) as CompositeAliasQueryResult;
        composite.ShouldSatisfyAllConditions(
            c => c.ShouldNotBeNull(),
            c => c.Aliases.ElementAt(index).Delay.ShouldBe(delay)
        );
    }

    [Fact]
    public void When_using_a_macro_Then_it_has_default_description()
    {
        QueryResult[] queryResults =
        [
            new AliasQueryResult { Name = "macro_1", FileName = "@multi@" },
            new AliasQueryResult { Name = "macro_2", FileName = "@multi@" },
            new AliasQueryResult { Name = "macro_3", FileName = "@multi@" }
        ];

        var serviceProvider = new ServiceCollection().AddMockSingleton<ILogger<MacroAliasExpanderService>>()
                                                     .AddMockSingleton<IAliasRepository>()
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddMockSingleton<IExecutionService>()
                                                     .AddMockSingleton<ISearchService>()
                                                     .AddSingleton(
                                                         new AssemblySource
                                                         {
                                                             MacroSource = Assembly.GetAssembly(typeof(MultiMacro))
                                                         }
                                                     )
                                                     .AddSingleton<MacroAliasExpanderService>()
                                                     .AddSingleton<AssemblySource>()
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddMacroServices()
                                                     .AddMockSingleton<IClipboardService>()
                                                     .AddMockSingleton<IGithubService>()
                                                     .AddMockSingleton<IUserGlobalNotificationService>()
                                                     .AddMockSingleton<IEnigma>()
                                                     .AddMockSingleton<ISection<GithubSection>>()
                                                     .AddLoggerFactoryForTests(OutputHelper)
                                                     .AddLogging()
                                                     .BuildServiceProvider();

        var output = serviceProvider.GetService<MacroAliasExpanderService>()
                                    .Expand(queryResults)
                                    .ToArray();

        Assert.Multiple(
            () => output.GetDoubloons().Count().ShouldBe(0),
            () => Assert.All(
                output,
                item => item.Description.ShouldNotBeNullOrWhiteSpace("Default description should be provided")
            )
        );
    }

    [Theory]
    [InlineData("un")]
    [InlineData("deux")]
    [InlineData("trois")]
    public void When_using_a_macro_Then_it_is_recognised_as_macro(string macro)
    {
        var query = new AliasQueryResult { FileName = $"@{macro}@" };

        query.IsMacro().ShouldBeTrue();
    }

    [Theory]
    [InlineData("multi", true)]
    [InlineData("MULTI", true)]
    [InlineData("macro", false)]
    [InlineData("MACRO", false)]
    [InlineData("calendar", false)]
    [InlineData("CALENDAR", false)]
    public void When_using_composite_macro_Then_it_is_marked_as_composite(string macro, bool expected)
    {
        var alias = new AliasQueryResult { FileName = $"@{macro}@" };

        alias.IsComposite().ShouldBe(expected);
    }

    [Fact]
    public void When_using_MacroQueryResult_Then_it_is_assignable_to_SelfExecutableQueryResult()
    {
        // ARRANGE
        var serviceProvider = new ServiceCollection()
                              .AddSingleton(
                                  new AssemblySource { MacroSource = Assembly.GetAssembly(typeof(MultiMacro))! }
                              )
                              .AddSingleton<ILoggerFactory, LoggerFactory>()
                              .AddMockSingleton<ILogger<MacroAliasExpanderService>>()
                              .AddMockSingleton<IExecutionService>()
                              .AddMockSingleton<ISearchService>()
                              .AddMockSingleton<IAliasRepository>()
                              .AddSingleton<MacroAliasExpanderService>()
                              .AddMockSingleton<IClipboardService>()
                              .AddLoggerFactoryForTests(OutputHelper)
                              .AddLogging()
                              .AddMockSingleton<IGithubService>()
                              .AddMockSingleton<IUserGlobalNotificationService>()
                              .AddMockSingleton<IEnigma>()
                              .AddMockSingleton<ISection<GithubSection>>()
                              .AddMacroServices()
                              .BuildServiceProvider();


        // ACT
        var macros = serviceProvider.GetServices<MacroQueryResult>()
                                    .ToList();

        // ASSERT

        macros.ShouldNotBeNull();
        macros!.Count.ShouldBeGreaterThan(0, "there are preconfigured macros");

        Assert.All(
            macros,
            macro =>
            {
                OutputHelper.WriteLine($"Checking '{macro.Name}'");
                macro.ShouldBeAssignableTo(typeof(SelfExecutableQueryResult));
                macro.ShouldBeAssignableTo(typeof(MacroQueryResult));
            }
        );
    }

    [Fact]
    public void When_using_multi_macro_Then_it_has_expected_name()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db, OutputHelper);

        //Act
        var results = service.Search("alias1");

        //Assert
        results.ElementAt(0).GetMacroName().ShouldBe("MULTI");
    }

    [Fact]
    public void When_using_Multi_macro_Then_it_is_assignable_to_CompositeAliasQueryResult()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db, OutputHelper);

        // Act
        var results = service.Search("alias1");

        // Assert
        results.ElementAt(0).ShouldBeAssignableTo<CompositeAliasQueryResult>();
    }

    [Fact]
    public void When_using_multi_macro_Then_it_is_recognised_as_macro()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db, OutputHelper);

        //Act
        var results = service.Search("alias1")
                             .ToArray();

        //Assert
        results.ShouldSatisfyAllConditions(
            r => r.Length.ShouldBe(1),
            r => r[0].IsMacro().ShouldBeTrue()
        );
    }

    [Fact]
    public void When_using_regular_alias_then_is_it_not_recognised_as_macro()
    {
        // Arrange
        const string sql = Cfg.SqlForAliases;
        using var db = BuildFreshDb(sql);
        var service = Cfg.GetDataService(db, OutputHelper);

        // Act
        var results = service.Search("alias2")
                             .ToArray();

        // Assert
        results.ShouldSatisfyAllConditions(
            r => r.Length.ShouldBe(1),
            r => r[0].IsMacro().ShouldBeFalse()
        );
    }

    #endregion

    #region Classes

    private static class Cfg
    {
        #region Fields

        public const string SqlForAliases = @"
                insert into alias (id, file_name, arguments) values (1000, '@multi@', '@alias2@@alias3');
                insert into alias_name (id, id_alias, name) values (1000, 1000, 'alias1');

                insert into alias (id, file_name) values (2000, 'c:\dummy\dummy.exe');
                insert into alias_name (id, id_alias, name) values (2000, 2000, 'alias2');

                insert into alias (id, file_name) values (3000, 'c:\dummy\dummy.exe');
                insert into alias_name (id, id_alias, name) values (3000, 3000, 'alias3');";

        #endregion

        #region Methods

        public static IAliasRepository GetDataService(IDbConnection db, ITestOutputHelper output)
        {
            var log = new MicrosoftLoggingLoggerFactory(output);
            var service = new SQLiteAliasRepository(
                new DbSingleConnectionManager(db),
                log,
                new DbActionFactory(log)
            );
            return service;
        }

        #endregion
    }

    #endregion Classes
}