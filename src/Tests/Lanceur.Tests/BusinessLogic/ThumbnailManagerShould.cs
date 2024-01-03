using AutoMapper;
using Dapper;
using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Tests.Logging;
using Lanceur.Tests.Mocks;
using Lanceur.Tests.SQL;
using Lanceur.Tests.SQLite;
using Lanceur.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic;

public class ThumbnailManagerShould : SQLiteTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion Fields

    #region Constructors

    public ThumbnailManagerShould(ITestOutputHelper output)
    {
        _output = output;
    }

    #endregion Constructors

    #region Methods

    [Fact]
    public async Task RefreshThumbnailsWithoutRemovingAdditionalParameters()
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

        var connectionMgr = new SQLiteSingleConnectionManager(BuildFreshDb(sql));
        var loggerFactory = new MicrosoftLoggingLoggerFactory(_output);

        var cfg = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();
        });
        var conversionService = new AutoMapperConverter(new Mapper(cfg));
        var dbRepository = new SQLiteRepository(connectionMgr, loggerFactory, conversionService);
        var thumbnailRefresher = new MockThumbnailRefresher();

        var thumbnailManager = new ThumbnailManager(loggerFactory, dbRepository, thumbnailRefresher);
        var aliases = dbRepository.Search("a");

        // ACT
        await thumbnailManager.RefreshThumbnailsAsync(aliases);

        // ASSERT
        connectionMgr.WithinTransaction(tx => (long)tx.Connection.ExecuteScalar("select count(*) from alias_argument"))
                     .Should().Be(6);
    }

    #endregion Methods
}