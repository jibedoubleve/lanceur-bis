using Dapper;
using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Services;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tools;
using Lanceur.Tests.Tools.SQL;
using Lanceur.Ui.Core.Utils;
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
        var sql = new SqlBuilder().AppendAlias(
                                      1,
                                      "fileName1",
                                      "some parameters 1",
                                      cfg: alias =>
                                      {
                                          alias.WithSynonyms("a1", "a2", "a3")
                                               .WithArgument("name_0", "argument_0")
                                               .WithArgument("name_0", "argument_0");
                                      }
                                  )
                                  .AppendAlias(
                                      110,
                                      "fileName2",
                                      "some parameters 2",
                                      cfg: alias =>
                                      {
                                          alias.WithSynonyms("aa1", "ab2", "ab3")
                                           .WithArgument("name_0", "argument_0")
                                           .WithArgument("name_0", "argument_0");
                                      }
                                  )
                                  .AppendAlias(
                                      120,
                                      "fileName3",
                                      "some parameters 3",
                                      cfg: alias =>
                                      {
                                          alias.WithSynonyms("ac1", "ac2", "ac3")
                                               .WithArgument("name_0", "argument_0")
                                               .WithArgument("name_0", "argument_0");
                                      }
                                  )
                                  .ToString();

        OutputHelper.WriteLine(sql);
        var connectionString = ConnectionStringFactory.InMemory.ToString();
        var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql, connectionString));
        var loggerFactory = new MicrosoftLoggingLoggerFactory(OutputHelper);

        var conversionService = new AutoMapperMappingService();
        var dbRepository = new SQLiteAliasRepository(
            connectionMgr,
            loggerFactory,
            conversionService,
            new DbActionFactory(new AutoMapperMappingService(), loggerFactory)
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