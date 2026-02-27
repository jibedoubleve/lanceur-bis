using Dapper;
using Lanceur.Core.Repositories;
using Shouldly;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Tests.Tools.SQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.Services;

public class ThumbnailServiceTest : TestBase
{
    #region Constructors

    public ThumbnailServiceTest(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Fact]
    public void When_refresh_thumbnails_Then_additional_parameters_are_not_removed()
    {
        // ARRANGE
        var sql = new SqlBuilder().AppendAlias(a => a.WithFileName("fileName1")
                                                     .WithArguments("some parameters 1")
                                                     .WithSynonyms("a1", "a2", "a3")
                                                     .WithAdditionalParameters(
                                                         ("name_0", "argument_0"),
                                                         ("name_0", "argument_0")))
                                  .AppendAlias(a => a.WithFileName("fileName2")
                                                     .WithArguments("some parameters 2")
                                                     .WithSynonyms("aa1", "ab2", "ab3")
                                                     .WithAdditionalParameters(
                                                         ("name_0", "argument_0"),
                                                         ("name_0", "argument_0")))
                                  .AppendAlias(a => a.WithFileName("fileName3")
                                                     .WithArguments("some parameters 3")
                                                     .WithSynonyms("ac1", "ac2", "ac3")
                                                     .WithAdditionalParameters(
                                                         ("name_0", "argument_0"),
                                                         ("name_0", "argument_0")))
                                  .ToSql();

        OutputHelper.WriteLine(sql);

        var sp = new ServiceCollection()
                 .AddSingleton<IDbConnectionManager>(_
                     => new DbSingleConnectionManager(BuildFreshDb(sql, ConnectionStringFactory.InMemory))
                 )
                 .AddSingleton<ILoggerFactory>(_ => new MicrosoftLoggingLoggerFactory(OutputHelper))
                 .AddSingleton<IAliasRepository, SQLiteAliasRepository>()
                 .AddSingleton<IDbActionFactory, DbActionFactory>()
                 .AddSingleton<IThumbnailService, ThumbnailService>()
                 .AddSingleton<IAliasManagementService, AliasManagementService>()
                 .AddMockSingleton<IPackagedAppSearchService>()
                 .AddMockSingleton<IFavIconService>()
                 .BuildServiceProvider();

        var dbRepository = sp.GetService<IAliasRepository>();
        var thumbnailService = sp.GetService<IThumbnailService>();
        var connectionMgr = sp.GetService<IDbConnectionManager>();
        
        var aliases = dbRepository.Search("a1");

        // ACT
        thumbnailService.UpdateThumbnail(aliases.First());

        // ASSERT
        connectionMgr.WithinTransaction(tx => (long)tx.Connection!.ExecuteScalar("select count(*) from alias_argument")!)
                     .ShouldBe(6);
    }

    #endregion
}