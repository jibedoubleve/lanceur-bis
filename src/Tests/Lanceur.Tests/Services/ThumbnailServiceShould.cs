using Dapper;
using FluentAssertions;
using Lanceur.Core.Mappers;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Services;

public class ThumbnailServiceShould : TestBase
{
    #region Constructors

    public ThumbnailServiceShould(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Fact]
    public void RefreshThumbnailsWithoutRemovingAdditionalParameters()
    {
        // ARRANGE
        var sql = new SqlGenerator().AppendAlias(a => a.WithFileName("fileName1")
                                                       .WithArguments("some parameters 1")
                                                       .WithSynonyms("a1", "a2", "a3")
                                                       .WithAdditionalParameters(
                                                           ("name_0", "argument_0"),
                                                           ("name_0", "argument_0"))
                                    )
                                    .AppendAlias(a => a.WithFileName("fileName2")
                                                       .WithArguments("some parameters 2")
                                                       .WithSynonyms("aa1", "ab2", "ab3")
                                                       .WithAdditionalParameters(
                                                           ("name_0", "argument_0"),
                                                           ("name_0", "argument_0"))
                                    )
                                    .AppendAlias(a => a.WithFileName("fileName3")
                                                       .WithArguments("some parameters 3")
                                                       .WithSynonyms("ac1", "ac2", "ac3")
                                                       .WithAdditionalParameters(
                                                           ("name_0", "argument_0"),
                                                           ("name_0", "argument_0"))
                                    )
                                    .GenerateSql();

        OutputHelper.WriteLine(sql);
        var connectionMgr = new DbSingleConnectionManager(
            BuildFreshDb(sql, ConnectionStringFactory.InMemory)
        );
        var loggerFactory = new MicrosoftLoggingLoggerFactory(OutputHelper);

        var conversionService = new MappingService();
        var dbRepository = new SQLiteAliasRepository(
            connectionMgr,
            loggerFactory,
            conversionService,
            new DbActionFactory(new MappingService(), loggerFactory)
        );

        var packagedAppSearchService = Substitute.For<IPackagedAppSearchService>();
        var favIconManager = Substitute.For<IFavIconService>();
        var thumbnailService = new ThumbnailService(
            loggerFactory,
            dbRepository,
            packagedAppSearchService,
            favIconManager
        );

        var aliases = dbRepository.Search("a");

        // ACT
        thumbnailService.UpdateThumbnails(aliases);

        // ASSERT
        connectionMgr.WithinTransaction(tx => (long)tx.Connection!.ExecuteScalar("select count(*) from alias_argument")!)
                     .Should()
                     .Be(6);
    }

    #endregion
}