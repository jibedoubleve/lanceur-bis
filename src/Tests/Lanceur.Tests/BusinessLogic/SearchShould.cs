using System.Data;
using System.Data.SQLite;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Stores;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Ui.Core.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic;

public class SearchShould : TestBase
{
    #region Fields

    private readonly ILoggerFactory _testLoggerFactory;

    private const string SqlCreateAlias = """
                                          insert into alias (id, file_name, arguments) values (1000, '@multi@', '@alias2@@alias3');
                                          insert into alias_name (id, id_alias, name) values (1000, 1000, 'alias1');

                                          insert into alias (id, file_name, arguments) values (2000, 'arg', 'c:\dummy\dummy.exe');
                                          insert into alias_name (id, id_alias, name) values (2000, 2000, 'alias2');

                                          insert into alias (id, file_name, arguments) values (3000, 'arg', 'c:\dummy\dummy.exe');
                                          insert into alias_name (id, id_alias, name) values (3000, 3000, 'alias3');
                                          """;

    #endregion

    #region Constructors

    public SearchShould(ITestOutputHelper output) : base(output) => _testLoggerFactory = new MicrosoftLoggingLoggerFactory(output);

    #endregion

    #region Methods

    [Fact]
    public void HaveStores()
    {
        var serviceProvider
            = new ServiceCollection().AddMockSingleton<ILoggerFactory>()
                                     .AddMockSingleton<ILogger<StoreLoader>>()
                                     .AddSingleton<AssemblySource>()
                                     .AddSingleton<IStoreLoader, StoreLoader>()
                                     .AddSingleton<IMacroManager, MacroManager>()
                                     .AddSingleton<SearchService>()
                                     .AddMockSingleton<ISearchServiceOrchestrator>()
                                     .AddMockSingleton<IThumbnailManager>()
                                     .BuildServiceProvider();


        var service = serviceProvider.GetService<SearchService>();
        service.Stores.Should().HaveCountGreaterThan(4);
    }

    [Fact]
    public void NOT_HaveNullParameters()
    {
        // arrange
        var converter = new AutoMapperMappingService();
        using var db = BuildFreshDb(SqlCreateAlias);
        using var conn = new DbSingleConnectionManager(db);

        var repository = new SQLiteAliasRepository(conn, _testLoggerFactory, converter, new DbActionFactory(new AutoMapperMappingService(), _testLoggerFactory));

        // act
        var results = repository.GetAll();
        var parameters = results.Select(c => c.Parameters);

        //assert
        parameters.Should().NotContain((string)null);
    }

    [Fact]
    public void NotSetUsageWhenCounterIsNegative()
    {
        /*
         * Create a new alias with usage set to -1
         * Execute it 4 times
         * Check counter is still -1
         */
        OutputHelper.Arrange();
        var sql = new SqlBuilder().AppendAlias(1)
                                  .AppendSynonyms(1, "a")
                                  .ToString();
        var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));
        var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
        var converter = Substitute.For<IMappingService>();
        QueryResult alias = new AliasQueryResult { Id = 1, Name = "a", Count = -1 };

        var repository = new SQLiteAliasRepository(connectionMgr, logger, converter, new DbActionFactory(new AutoMapperMappingService(), logger));

        OutputHelper.Act();

        for (var i = 0; i < 5; i++) repository.SetUsage(alias);

        OutputHelper.Assert();
        const string sqlCount = "select count(*) from alias_usage where id_alias = 1";

        connectionMgr.WithinTransaction(x => x.Connection!.ExecuteScalar<int>(sqlCount))
                     .Should()
                     .Be(0);
    }

    [Fact]
    public async Task ReturnNoResultMessageWhenMisconfiguredMacro()
    {
        // ARRANGE
        const string sql = SqlCreateAlias +
                           """
                           insert into alias (id, file_name) values (4000, '@zzzz@');
                           insert into alias_name (id, id_alias, name) values (4000, 4000, 'zz');
                           """;

        using var db = BuildFreshDb(sql);
        using var conn = new DbSingleConnectionManager(db);

        var serviceProvider = new ServiceCollection().AddMockSingleton<IThumbnailManager>()
                                                     .AddLogger<StoreLoader>(OutputHelper)
                                                     .AddLogger<MacroManager>(OutputHelper)
                                                     .AddSingleton<ILoggerFactory, LoggerFactory>()
                                                     .AddSingleton<IMacroManager, MacroManager>()
                                                     .AddSingleton(Substitute.For<ISearchServiceOrchestrator>())
                                                     .AddSingleton<IMappingService, AutoMapperMappingService>()
                                                     .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                                                     .AddSingleton<IDbConnectionManager, DbSingleConnectionManager>()
                                                     .AddSingleton<IDbConnection, SQLiteConnection>()
                                                     .AddSingleton<SearchService>()
                                                     .AddSingleton<AssemblySource>()
                                                     .AddSingleton<IDbActionFactory, DbActionFactory>()
                                                     .AddMockSingleton<IStoreLoader>(
                                                         (serviceProvider, storeLoader) =>
                                                         {
                                                             storeLoader.Load().Returns([new AliasStore(serviceProvider)]);
                                                             return storeLoader;
                                                         }
                                                     )
                                                     .BuildServiceProvider();

        // ACT
        var service = serviceProvider.GetService<SearchService>();
        var result = (await service.SearchAsync(new("z"))).ToArray();

        // ASSERT
        using (new AssertionScope())
        {
            result.Should().HaveCountGreaterThan(0);
            result[0].Name.Should().Be("No result found");
        }
    }

    [Fact]
    public async Task ReturnResultWithExactMatchOnTop()
    {
        var dt = DateTime.Now;
        var converter = Substitute.For<IMappingService>();
        var sql = $"""
                   insert into alias (id, file_name, arguments) values (1000, 'un', '@alias2@@alias3');
                   insert into alias_name (id, id_alias, name) values (1001, 1000, 'un');

                   insert into alias (id, file_name, arguments) values (2000, 'deux', '@alias2@@alias3');
                   insert into alias_name (id, id_alias, name) values (1002, 2000, 'deux');

                   insert into alias (id, file_name, arguments) values (3000, 'trois', '@alias2@@alias3');
                   insert into alias_name (id, id_alias, name) values (1003, 3000, 'trois');
                   --
                   insert into alias (id, file_name, arguments) values (4000, 'u', '@alias2@@alias3');
                   insert into alias_name (id, id_alias, name) values (1004, 4000, 'u');
                   --
                   insert into alias_usage (id_alias, time_stamp) values (1000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   insert into alias_usage (id_alias, time_stamp) values (1000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   insert into alias_usage (id_alias, time_stamp) values (1000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   ---
                   insert into alias_usage (id_alias, time_stamp) values (2000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   insert into alias_usage (id_alias, time_stamp) values (2000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   insert into alias_usage (id_alias, time_stamp) values (2000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   ---
                   insert into alias_usage (id_alias, time_stamp) values (3000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   insert into alias_usage (id_alias, time_stamp) values (3000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   insert into alias_usage (id_alias, time_stamp) values (3000, '{dt.AddMinutes(1):yyyy-MM-dd HH:m:s}');
                   """;

        // ARRANGE
        using var db = BuildFreshDb(sql);
        using var conn = new DbSingleConnectionManager(db);
        const string criterion = "u";

        var serviceProvider = new ServiceCollection().AddMemoryDb(conn)
                                                     .AddLogging(builder => builder.AddXUnit(OutputHelper))
                                                     .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                                                     .AddSingleton(_testLoggerFactory)
                                                     .AddSingleton(converter)
                                                     .AddSingleton(Substitute.For<IStoreLoader>())
                                                     .AddSingleton<ISearchService, SearchService>()
                                                     .AddSingleton<IDbActionFactory, DbActionFactory>()
                                                     .AddMockSingleton<IThumbnailManager>()
                                                     .AddSingleton<IMemoryCache, MemoryCache>()
                                                     .AddMockSingleton<IStoreLoader>(
                                                         (sp, _) =>
                                                         {
                                                             var stores = sp.GetService<IStoreLoader>();
                                                             stores.Load().Returns([new AliasStore(sp)]);
                                                             return stores;
                                                         }
                                                     )
                                                     .AddMockSingleton<IMacroManager>(
                                                         (sp, macroManager) =>
                                                         {
                                                             var results = sp.GetService<IAliasRepository>()
                                                                             .Search(criterion)
                                                                             .ToList();
                                                             macroManager.Handle(Arg.Any<QueryResult[]>())
                                                                         .Returns(results);
                                                             return macroManager;
                                                         }
                                                     )
                                                     .AddMockSingleton<ISearchServiceOrchestrator>(
                                                         (_, orchestrator) =>
                                                         {
                                                             orchestrator.IsAlive(Arg.Any<IStoreService>(), Arg.Any<Cmdline>())
                                                                         .Returns(true);
                                                             return orchestrator;
                                                         }
                                                     )
                                                     .BuildServiceProvider();

        // ACT
        var searchService = serviceProvider.GetService<ISearchService>();
        var result = (await searchService.SearchAsync(new(criterion))).ToArray();

        // ASSERT
        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("u");
            result.First().Id.Should().Be(4000);
        }
    }

    [Fact]
    public async Task ReturnValues()
    {
        var serviceProvider = new ServiceCollection().AddMockSingleton<IMacroManager>()
                                                     .AddMockSingleton<IThumbnailManager>()
                                                     .AddMockSingleton<IStoreLoader>()
                                                     .AddMockSingleton<ILoggerFactory>()
                                                     .AddMockSingleton<ISearchServiceOrchestrator>(
                                                         (_, orchestrator) =>
                                                         {
                                                             orchestrator.IsAlive(Arg.Any<IStoreService>(), Arg.Any<Cmdline>())
                                                                         .Returns(true);
                                                             return orchestrator;
                                                         }
                                                     )
                                                     .AddTransient<SearchService>()
                                                     .BuildServiceProvider();
        var query = new Cmdline("code");
        var service = serviceProvider.GetService<SearchService>();

        var result = (await service.SearchAsync(query)).ToArray();

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result.ElementAt(0).IsResult.Should().BeFalse();
        }
    }

    [Fact]
    public void SetUsageDoesNotResetAdditionalParameters()
    {
        OutputHelper.Arrange();
        var sql = new SqlBuilder().AppendAlias(1)
                                  .AppendSynonyms(1, "a")
                                  .AppendArgument(1)
                                  .AppendArgument(1)
                                  .AppendArgument(1)
                                  .ToString();

        var connectionManager = new DbSingleConnectionManager(BuildFreshDb(sql));
        var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
        var converter = Substitute.For<IMappingService>();
        QueryResult alias = new AliasQueryResult { Id = 1, Name = "a" };

        var repository = new SQLiteAliasRepository(connectionManager, logger, converter, new DbActionFactory(new AutoMapperMappingService(), logger));

        OutputHelper.Act();
        repository.SetUsage(alias);

        OutputHelper.Assert();
        const string sqlCount = "select count(*) from alias_argument";

        connectionManager.WithinTransaction(x => x.Connection!.ExecuteScalar<int>(sqlCount))
                         .Should()
                         .Be(3);
    }

    #endregion
}