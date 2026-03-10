using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
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

    private static IEnumerable<object[]> GetStrategies()
    {
        yield return
        [
            "https://example.com",
            typeof(FavIconAppThumbnailStrategy),
            (Action<IServiceCollection>)(s => s.AddMockSingleton<IFavIconService>((_, i) => {
                    i.UpdateFaviconAsync(
                         Arg.Any<AliasQueryResult>(),
                         Arg.Any<Func<string, string>>(),
                         Arg.Any<CancellationToken>()
                     )!
                     .Returns(Task.FromResult<string?>("new_thumbnail_path"));
                    return i;
                })
            )
        ];
        yield return
        [
            "package:some.app.id",
            typeof(PackagedAppThumbnailStrategy),
            (Action<IServiceCollection>)(s => s.AddMockSingleton<IPackagedAppSearchService>((_, i) => {
                    i.GetByInstalledDirectoryAsync(Arg.Any<string>())
                     .Returns(Task.FromResult(Enumerable.Empty<PackagedApp>()));
                    return i;
                })
            )
        ];
    }

    [Theory]
    [InlineData("old_thumbnail", "old_thumbnail")]
    [InlineData("old_thumbnail", "new_thumbnail")]
    public async Task When_refresh_favicon_thumbnails_Then_only_update_db_when_changes(string oldValue, string newValue)
    {
        // ARRANGE
        const string name = "alias_name";
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms(name)
                                                     .WithThumbnail(oldValue)
                                  )
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
                 .AddMockSingleton<IFavIconService>((_, i) => {
                         i.UpdateFaviconAsync(
                              Arg.Any<AliasQueryResult>(),
                              Arg.Any<Func<string, string>>(),
                              Arg.Any<CancellationToken>())!
                          .Returns(Task.FromResult(newValue));
                         return i;
                     }
                 )
                 .AddLoggingForTests(OutputHelper)
                 .BuildServiceProvider();

        var conn = sp.GetService<IDbConnectionManager>()!;
        var repo = sp.GetService<IAliasRepository>()!;
        var strategy = sp.GetService<IThumbnailStrategy>()!;
        using var source = new CancellationTokenSource();

        // ACT
        var alias = repo.GetAll().Single();

        await strategy.UpdateThumbnailAsync(alias, source.Token);

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
    public void When_searching_alias_Then_additional_are_not_loaded()
    {
        // ARRANGE
        const string name = "alias_name";
        var sql = new SqlBuilder().AppendAlias(a => a.WithSynonyms(name)
                                                     .WithAdditionalParameters(
                                                         ("name_0", "argument_0"),
                                                         ("name_1", "argument_1")
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
                 .AddSingleton<IAliasManagementService, AliasManagementService>()
                 .AddLoggingForTests(OutputHelper)
                 .BuildServiceProvider();

        var dbRepository = sp.GetService<IAliasRepository>()!;

        // ACT
        var aliasesLight = dbRepository.Search(name).ToList();

        // ASSERT
        aliasesLight.ShouldSatisfyAllConditions(
            a => a.Count.ShouldBe(1),
            a => a[0].AdditionalParameters.Count.ShouldBe(0)
        );
    }

    [Theory]
    [MemberData(nameof(GetStrategies))]
    public async Task When_strategy_updates_thumbnail_Then_additional_parameters_are_not_removed(
        string fileName, Type strategyType, Action<IServiceCollection> mockSetup)
    {
        // ARRANGE
        const string name = "alias_name";
        var sql = new SqlBuilder()
                  .AppendAlias(a => a.WithSynonyms(name)
                                     .WithFileName(fileName)
                                     .WithAdditionalParameters(
                                         ("name_0", "argument_0"),
                                         ("name_1", "argument_1")
                                     )
                  )
                  .ToSql();

        OutputHelper.WriteLine(sql);

        var services = new ServiceCollection()
                       .AddSingleton<IDbConnectionManager>(_ =>
                           new DbSingleConnectionManager(BuildFreshDb(sql, ConnectionStringFactory.InMemory))
                       )
                       .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                       .AddSingleton<IDbActionFactory, DbActionFactory>()
                       .AddSingleton(typeof(IThumbnailStrategy), strategyType)
                       .AddSingleton<IAliasManagementService, AliasManagementService>()
                       .AddLoggingForTests(OutputHelper);

        mockSetup(services);

        var sp = services.BuildServiceProvider();
        var conn = sp.GetService<IDbConnectionManager>()!;
        var repo = sp.GetService<IAliasRepository>()!;
        var strategy = sp.GetService<IThumbnailStrategy>()!;

        // ACT
        var alias = repo.GetAll().Single();
        await strategy.UpdateThumbnailAsync(alias, CancellationToken.None);

        // ASSERT
        conn.WithConnection(c => (long)c.ExecuteScalar("select count(*) from alias_argument")!)
            .ShouldBe(2);
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

        var dbRepository = sp.GetService<IAliasRepository>()!;

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