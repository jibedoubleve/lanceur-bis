using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Helpers;
using Lanceur.Infra.Win32.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Infra.Win32.Thumbnails.Strategies;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public class ThumbnailServiceTest : TestBase
{
    #region Constructors

    public ThumbnailServiceTest(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Theory]
    [InlineData("old_thumbnail", "old_thumbnail")]
    [InlineData("old_thumbnail", "new_thumbnail")]
    public async Task When_refresh_favicon_thumbnails_Then_only_update_db_when_changes(string oldValue, string newValue)
    {
        // ARRANGE
        const string name = "alias_name";
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms(name)
                                                     .WithThumbnail(oldValue))
                                  .ToSql();

        OutputHelper.WriteLine(sql);

        var sp = new ServiceCollection()
                 .AddSingleton<IDbConnectionManager>(_ =>
                     new DbSingleConnectionManager(BuildFreshDb(sql, ConnectionStringFactory.InMemory))
                 )
                 .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                 .AddSingleton<IDbActionFactory, DbActionFactory>()
                 .AddSingleton<IThumbnailStrategy, FavIconAppThumbnailStrategy>()
                 .AddSingleton<IAliasManagementService, AliasManagementService>()
                 .AddMockSingleton<IFavIconService>((_, i) =>
                 {
                     i.UpdateFaviconAsync(Arg.Any<AliasQueryResult>(), Arg.Any<Func<string, string>>())
                      .Returns(Task.FromResult(newValue));
                     return i;
                 })
                 .AddLoggingForTests(OutputHelper)
                 .BuildServiceProvider();
        
        var conn = sp.GetService<IDbConnectionManager>();
        var repo =  sp.GetService<IAliasRepository>();
        var strategy = sp.GetService<IThumbnailStrategy>();

        // ACT
        var alias = repo.GetAll().Single();
        
        await strategy.UpdateThumbnailAsync(alias);

        // ASSERT
        const string sql2 = """
                            select 
                                an.name,
                                a.thumbnail 
                            from alias a
                                 inner join alias_name an on a.id = an.id_alias
                            """;
        var aliases = conn.WithConnection(c => c.Query<AliasQueryResult>(sql2))
                          .ToList();
        
        aliases.ShouldSatisfyAllConditions(
            a => a.Count.ShouldBe(1),
            a => a[0].Name.ShouldBe(name),
            a => a[0].Thumbnail.ShouldBe(newValue)
        );

    }

    [Fact]
    public void When_refresh_thumbnails_Then_additional_parameters_are_not_removed()
    {
        // ARRANGE
        var sql = new SqlBuilder().AppendAlias(a => a.WithFileName("fileName1")
                                                     .WithArguments("some parameters 1")
                                                     .WithSynonyms("a1", "a2", "a3")
                                                     .WithAdditionalParameters(
                                                         ("name_0", "argument_0"),
                                                         ("name_1", "argument_1")
                                                     )
                                  )
                                  .AppendAlias(a => a.WithFileName("fileName2")
                                                     .WithArguments("some parameters 2")
                                                     .WithSynonyms("aa1", "ab2", "ab3")
                                                     .WithAdditionalParameters(
                                                         ("name_2", "argument_2"),
                                                         ("name_3", "argument_3")
                                                     )
                                  )
                                  .AppendAlias(a => a.WithFileName("fileName3")
                                                     .WithArguments("some parameters 3")
                                                     .WithSynonyms("ac1", "ac2", "ac3")
                                                     .WithAdditionalParameters(
                                                         ("name_4", "argument_4"),
                                                         ("name_5", "argument_5")
                                                     )
                                  )
                                  .ToSql();

        OutputHelper.WriteLine(sql);

        var sp = new ServiceCollection()
                 .AddSingleton<IDbConnectionManager>(_
                     => new DbSingleConnectionManager(BuildFreshDb(sql, ConnectionStringFactory.InMemory))
                 )
                 .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                 .AddSingleton<IDbActionFactory, DbActionFactory>()
                 .AddSingleton<IThumbnailService, ThumbnailService>()
                 .AddSingleton<IAliasManagementService, AliasManagementService>()
                 .AddMockSingleton<IPackagedAppSearchService>()
                 .AddMockSingleton<IFavIconService>()
                 .AddStaThreadRunner()
                 .AddThumbnailStrategies()
                 .AddLoggingForTests(OutputHelper)
                 .BuildServiceProvider();

        var dbRepository = sp.GetService<IAliasRepository>();
        var thumbnailService = sp.GetService<IThumbnailService>();
        var connectionMgr = sp.GetService<IDbConnectionManager>();

        var aliases = dbRepository.Search("a1");

        // ACT
        thumbnailService.UpdateThumbnail(aliases.First());

        // ASSERT
        connectionMgr.WithConnection(conn => 
            (long)conn.ExecuteScalar("select count(*) from alias_argument")!
        ).ShouldBe(6);
    }
    
    [Fact]
    public void When_searching_alias_Then_additional_are_not_loaded()
    {
        // ARRANGE
        const string name = "alias_name";
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms(name)
                                                     .WithAdditionalParameters(
                                                         ("name_0", "argument_0"),
                                                         ("name_1", "argument_1")
                                                     )).ToSql();

        OutputHelper.WriteLine(sql);

        var sp = new ServiceCollection()
                 .AddSingleton<IDbConnectionManager>(_
                     => new DbSingleConnectionManager(BuildFreshDb(sql, ConnectionStringFactory.InMemory))
                 )
                 .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                 .AddSingleton<IDbActionFactory, DbActionFactory>()
                 .AddSingleton<IAliasManagementService, AliasManagementService>()
                 .AddLoggingForTests(OutputHelper)
                 .BuildServiceProvider();

        var dbRepository = sp.GetService<IAliasRepository>();

        // ACT
        var aliasesLight = dbRepository.Search(name).ToList();

        // ASSERT
        aliasesLight.ShouldSatisfyAllConditions(
            a => a.Count.ShouldBe(1),
            a => a[0].AdditionalParameters.Count.ShouldBe(0)
        );
    }
    
    [Fact]
    public void When_update_thumbnail_Then_it_is_saved_in_db()
    {
        // ARRANGE
        const string name = "alias_name";
        const string thumbnail = "thumbnail_name";
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms(name))
                                                     .ToSql();

        OutputHelper.WriteLine(sql);

        var sp = new ServiceCollection()
                 .AddSingleton<IDbConnectionManager>(_
                     => new DbSingleConnectionManager(BuildFreshDb(sql, ConnectionStringFactory.InMemory))
                 )
                 .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                 .AddSingleton<IDbActionFactory, DbActionFactory>()
                 .AddSingleton<IAliasManagementService, AliasManagementService>()
                 .AddLoggingForTests(OutputHelper)
                 .BuildServiceProvider();

        var dbRepository = sp.GetService<IAliasRepository>();

        // ACT
        var alias = dbRepository.Search(name).Single();
        alias.Thumbnail = thumbnail;
        dbRepository.UpdateThumbnail(alias);
        
        var found = dbRepository.Search(name).Single();

        // ASSERT
        found.ShouldSatisfyAllConditions(
            f => f.Thumbnail.ShouldNotBeNullOrEmpty(),
            f => f.Thumbnail.ShouldBe(thumbnail)
        );

    }
    
    #endregion
}