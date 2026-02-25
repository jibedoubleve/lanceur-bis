using System.Data;
using System.Data.SQLite;
using Dapper;
using Lanceur.Core;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public class SearchServiceTest : TestBase
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

    public SearchServiceTest(ITestOutputHelper output) : base(output)
        => _testLoggerFactory = new MicrosoftLoggingLoggerFactory(output);

    #endregion

    #region Methods

    [Fact]
    public void When_retrieving_aliases_from_repository_Then_parameters_are_mapped_as_expected()
    {
        // arrange
        using var db = BuildFreshDb(SqlCreateAlias);
        using var conn = new DbSingleConnectionManager(db);

        var repository = new SQLiteAliasRepository(
            conn,
            _testLoggerFactory,
            new DbActionFactory(_testLoggerFactory)
        );

        // act
        var results = repository.GetAll();
        var parameters = results.Select(c => c.Parameters);

        //assert
        parameters.ShouldNotContain((string)null);
    }

    [Fact]
    public void When_alias_has_negative_counter_Then_usage_is_ignored()
    {
        /*
         * Create a new alias with usage set to -1
         * Execute it 4 times
         * Check counter is still -1
         */
        OutputHelper.Arrange();
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms("a", "b"))
                  .ToSql();
        
        var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));
        var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
        QueryResult alias = new AliasQueryResult { Id = 1, Name = "a", Count = -1 };

        var repository = new SQLiteAliasRepository(
            connectionMgr,
            logger,
            new DbActionFactory(logger)
        );

        OutputHelper.Act();

        for (var i = 0; i < 5; i++) repository.SetUsage(alias);

        OutputHelper.Assert();
        const string sqlCount = "select count(*) from alias_usage where id_alias = 1";

        connectionMgr.WithinTransaction(x => x.Connection!.ExecuteScalar<int>(sqlCount)).ShouldBe(0);
    }

    [Fact]
    public async Task When_macro_is_misconfigured_Then_it_is_excluded_from_result()
    {
        // ARRANGE
        const string sql = SqlCreateAlias +
                           """
                           insert into alias (id, file_name) values (4000, '@zzzz@');
                           insert into alias_name (id, id_alias, name) values (4000, 4000, 'zz');
                           """;

        using var db = BuildFreshDb(sql);
        using var conn = new DbSingleConnectionManager(db);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMockSingleton<IThumbnailService>()
                         .AddTestOutputHelper(OutputHelper)
                         .AddSingleton<IStoreOrchestrationFactory>(
                             new StoreOrchestrationFactory()
                         )
                         .AddSingleton<ILoggerFactory, LoggerFactory>()
                         .AddSingleton<IMacroAliasExpanderService, MacroAliasExpanderService>()
                         .AddSingleton(Substitute.For<ISearchServiceOrchestrator>())
                         .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                         .AddSingleton<IDbConnectionManager, DbSingleConnectionManager>()
                         .AddSingleton<IDbConnection, SQLiteConnection>()
                         .AddSingleton<SearchService>()
                         .AddSingleton<AssemblySource>()
                         .AddSingleton<IDbActionFactory, DbActionFactory>();

        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, AliasStore>());
        serviceCollection.AddTestOutputHelper(OutputHelper);
        var serviceProvider = serviceCollection.BuildServiceProvider();


        // ACT
        var service = serviceProvider.GetService<SearchService>();
        var result = (await service.SearchAsync(new("z"))).ToArray();

        // ASSERT
        Assert.Multiple(
            () => result.Length.ShouldBeGreaterThan(0),
            () => result[0].Name.ShouldBe("No result found")
        );
    }

    [Fact]
    public async Task When_there_is_result_with_exact_match_Then_it_is_moved_on_top_of_the_list()
    {
        var dt = DateTime.Now;
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms("un")
                                     .WithArguments("@alias2@@alias3")
                                     .WithFileName("un")
                                     .WithUsage(dt.AddMinutes(1), dt.AddMinutes(1), dt.AddMinutes(1))
                  )
                  .AppendAlias(a => a.WithSynonyms("deux")
                                     .WithArguments("@alias2@@alias3")
                                     .WithFileName("deux")
                                     .WithUsage(dt.AddMinutes(1), dt.AddMinutes(1), dt.AddMinutes(1))
                  )
                  .AppendAlias(a => a.WithSynonyms("trois")
                                     .WithArguments("@alias2@@alias3")
                                     .WithFileName("trois")
                                     .WithUsage(dt.AddMinutes(1), dt.AddMinutes(1), dt.AddMinutes(1))
                  )
                  .AppendAlias(a => a.WithSynonyms("u")
                                     .WithArguments("@alias2@@alias3")
                                     .WithFileName("u")
                                     .WithUsage(dt.AddMinutes(1), dt.AddMinutes(1), dt.AddMinutes(1))
                  )
                  .ToSql();

        // ARRANGE
        using var db = BuildFreshDb(sql);
        using var conn = new DbSingleConnectionManager(db);
        const string criterion = "u";

        var sc = new ServiceCollection();
        sc.AddDatabase(conn)
          .AddLogging(builder => builder.AddXUnit(OutputHelper))
          .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
          .AddMockSingleton<IConfigurationFacade>()
          .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
          .AddSingleton(_testLoggerFactory)
          .AddSingleton<ISearchService, SearchService>()
          .AddSingleton<IDbActionFactory, DbActionFactory>()
          .AddMockSingleton<IThumbnailService>()
          .AddMockSingleton<IMacroAliasExpanderService>((sp, macroManager) =>
              {
                  var results = sp.GetService<IAliasRepository>()
                                  .Search(criterion)
                                  .ToList();
                  macroManager.Expand(Arg.Any<QueryResult[]>())
                              .Returns(results);
                  return macroManager;
              }
          )
          .AddMockSingleton<ISearchServiceOrchestrator>((_, orchestrator) =>
              {
                  orchestrator.IsAlive(
                                  Arg.Any<IStoreService>(),
                                  Arg.Any<Cmdline>()
                              )
                              .Returns(true);
                  return orchestrator;
              }
          );
        sc.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, AliasStore>());

        var serviceProvider = sc.BuildServiceProvider();

        // ACT
        var searchService = serviceProvider.GetService<ISearchService>();
        var result = (await searchService.SearchAsync(new(criterion))).ToArray();

        // ASSERT
        result.ShouldSatisfyAllConditions(
            r => r.Length.ShouldBe(2),
            r => r[0].Name.ShouldBe("u")
        );
    }

    [Fact]
    public async Task When_search_has_no_result_Then_result_is_informational_QueryResult()
    {
        var serviceProvider = new ServiceCollection().AddMockSingleton<IMacroAliasExpanderService>()
                                                     .AddTestOutputHelper(OutputHelper)
                                                     .AddTransient<SearchService>()
                                                     .AddSingleton<IStoreService, EverythingStore>()
                                                     .AddSingleton<IStoreOrchestrationFactory, StoreOrchestrationFactory>()
                                                     .AddSingleton<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
                                                     .AddStoreServicesMockContext()
                                                     .AddStoreServicesConfiguration()
                                                     .BuildServiceProvider();
        var query = new Cmdline("code");
        var service = serviceProvider.GetService<SearchService>();

        var result = (await service.SearchAsync(query)).ToArray();

        result.ShouldSatisfyAllConditions(
            r => r.Length.ShouldBe(1),
            r => r[0].IsResult.ShouldBeFalse()
        );
    }

    [Fact]
    public void When_setting_usage_Then_it_does_not_reset_additional_parameters()
    {
        OutputHelper.Arrange();
        var sql = new SqlBuilder().AppendAlias(a =>
                                    {
                                        a.WithSynonyms("a")
                                         .WithAdditionalParameters()
                                         .WithAdditionalParameters()
                                         .WithAdditionalParameters();
                                    })
                                    .ToSql();

        var connectionManager = new DbSingleConnectionManager(BuildFreshDb(sql));
        var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
        QueryResult alias = new AliasQueryResult { Id = 1, Name = "a" };

        var repository = new SQLiteAliasRepository(
            connectionManager,
            logger,
            new DbActionFactory(logger)
        );

        OutputHelper.Act();
        repository.SetUsage(alias);

        OutputHelper.Assert();
        const string sqlCount = "select count(*) from alias_argument";

        connectionManager.WithinTransaction(x => x.Connection!.ExecuteScalar<int>(sqlCount))
                         .ShouldBe(3);
    }

    #endregion
}