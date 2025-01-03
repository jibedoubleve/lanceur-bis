using Dapper;
using FluentAssertions;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.DbActions;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tooling.Mocks;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Ui.Core.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic;

public class ThumbnailManagerShould : TestBase
{
    #region Constructors

    public ThumbnailManagerShould(ITestOutputHelper output) : base(output) { }

    #endregion

    #region Methods

    [Fact]
    public void RefreshThumbnailsWithoutRemovingAdditionalParameters()
    {
        // ARRANGE
        var sql = new SqlBuilder().AppendAlias(1, "fileName1", "arguments1")
                                  .AppendSynonyms(1, "a1", "a2", "a3")
                                  .AppendArgument(1, "name_0", "argument_0")
                                  .AppendArgument(1, "name_0", "argument_0")
                                  //--
                                  .AppendAlias(110, "fileName2", "arguments2")
                                  .AppendSynonyms(110, "aa1", "ab2", "ab3")
                                  .AppendArgument(110, "name_0", "argument_0")
                                  .AppendArgument(110, "name_0", "argument_0")
                                  //--
                                  .AppendAlias(120, "fileName3", "arguments3")
                                  .AppendSynonyms(120, "ac1", "ac2", "ac3")
                                  .AppendArgument(120, "name_0", "argument_0")
                                  .AppendArgument(120, "name_0", "argument_0")
                                  .ToString();

        var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));
        var loggerFactory = new MicrosoftLoggingLoggerFactory(OutputHelper);

        var conversionService = new AutoMapperMappingService();
        var dbRepository = new SQLiteRepository(connectionMgr, loggerFactory, conversionService, new DbActionFactory(new AutoMapperMappingService(), loggerFactory));
        var thumbnailRefresher = new MockThumbnailRefresher();

        var thumbnailManager = new ThumbnailManager(loggerFactory, dbRepository, thumbnailRefresher);
        var aliases = dbRepository.Search("a");

        // ACT
        thumbnailManager.RefreshThumbnails(aliases);

        // ASSERT
        connectionMgr.WithinTransaction(tx => (long)tx.Connection.ExecuteScalar("select count(*) from alias_argument"))
                     .Should()
                     .Be(6);
    }

    #endregion
}