using AutoMapper;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Humanizer;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.Tests.Logging;
using Lanceur.Tests.Mocks;
using Lanceur.Tests.SQL;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Utils.Builders;
using Lanceur.Utils;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class KeywordViewModelShould : SQLiteTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion Fields

    #region Constructors

    public KeywordViewModelShould(ITestOutputHelper output)
    {
        _output = output;
    }

    #endregion Constructors

    #region Methods

    [Fact]
    public void BeAbleToRemoveSynonym()=> new TestScheduler().With(scheduler =>
    {
        // ARRANGE
        const long idAlias = 10;
        var sql = new SqlBuilder().AppendAlias(idAlias, "@multi@", "@alias2@@alias3")
                                  .AppendSynonyms(idAlias, "multi1", "multi2", "multi3")
                                  .AppendAlias(20, "alias2", "action1")
                                  .AppendSynonyms(20, "alias2")
                                  .AppendAlias(30, "alias3", "action2")
                                  .AppendSynonyms(30, "alias3")
                                  .ToString();

        var connectionMgr = new SQLiteSingleConnectionManager(BuildFreshDb(sql));

        var logger = new XUnitLoggerFactory(_output);
        var cfg = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();
        });
        var conversionService = new AutoMapperConverter(new Mapper(cfg));
        var dbRepository = new SQLiteRepository(connectionMgr, logger, conversionService);

        var vm = new KeywordsViewModelBuilder()
                 .With(scheduler)
                 .With(logger)
                 .With(dbRepository)
                 .Build();

        // ACT
        vm.Activate(new());
        vm.SearchQuery = "multi";
        scheduler.AdvanceBy(200.Milliseconds().Ticks);

        vm.SelectedAlias.Synonyms = "multi1, multi2";
        vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();
        scheduler.Start();

        // ASSERT
        using (new AssertionScope())
        {
            // The database should have one less synonym
            var countSql = $"select count(*) from alias_name where id_alias = {idAlias}";
            connectionMgr.WithinTransaction(tx => (long)tx.Connection.ExecuteScalar(countSql))
                         .Should().Be(2);
            // And the UI also...
            vm.Aliases.Should().HaveCount(2);
        }
    });

    [Fact]
    public void CreateAliasAndSelectIt() => new TestScheduler().With(scheduler =>
    {
        // ARRANGE
        var dbRepository = Substitute.For<IDbRepository>();
        dbRepository.SelectNames(Arg.Any<string[]>())
                    .Returns(new ExistingNameResponse(Array.Empty<string>()));

        var vm = new KeywordsViewModelBuilder()
                 .With(scheduler)
                 .With(_output)
                 .With(dbRepository)
                 .Build();

        var synonyms = Guid.NewGuid().ToString();
        var fileName = Guid.NewGuid().ToString();

        // ACT

        vm.Activate(new());
        scheduler.Start();
        vm.CreatingAlias.Execute().Subscribe();
        var hash = vm.SelectedAlias.GetHashCode();

        vm.SelectedAlias.Synonyms = synonyms;
        vm.SelectedAlias.FileName = fileName;

        vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();

        // ASSERT
        vm.SelectedAlias.GetHashCode().Should().Be(hash);
    });

    [Fact]
    public void CreateAliasForPackagedApp() => new TestScheduler().With(scheduler =>
    {
        // ARRANGE
        var packagedAppSearchService = Substitute.For<IPackagedAppSearchService>();
        packagedAppSearchService.GetByInstalledDirectory(Arg.Any<string>())
                                .Returns(new List<PackagedApp>
                                             { new() { AppUserModelId = Guid.NewGuid().ToString() } });

        var vm = new KeywordsViewModelBuilder().With(scheduler)
                                               .With(_output)
                                               .With(packagedAppSearchService)
                                               .Build();

        // ACT
        vm.Activate(new());
        vm.CreatingAlias.Execute().Subscribe();
        vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();

        // ASSERT
        vm.SelectedAlias.FileName.Should().StartWith("package:");
    });

    [Theory]
    [InlineData("multi, ")]
    [InlineData("multi,")]
    public void NotBeAbleToCreateEmptyAlias(string synonyms) => new TestScheduler().With(scheduler =>
    {
        // ARRANGE
        var sql = new SqlBuilder().AppendAlias(10, "@multi@", "@alias2@@alias3")
                                  .AppendSynonyms(10, "multi1", "multi2", "multi3")
                                  .AppendAlias(20, "alias2", "action1")
                                  .AppendSynonyms(20, "alias2")
                                  .AppendAlias(30, "alias3", "action2")
                                  .AppendSynonyms(30, "alias3")
                                  .ToString();

        var connectionMgr = new SQLiteSingleConnectionManager(BuildFreshDb(sql));

        var logger = new XUnitLoggerFactory(_output);
        var cfg = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();
        });
        var conversionService = new AutoMapperConverter(new Mapper(cfg));
        var dbRepository      = new SQLiteRepository(connectionMgr, logger, conversionService);

        var vm = new KeywordsViewModelBuilder()
                 .With(scheduler)
                 .With(logger)
                 .With(dbRepository)
                 .Build();
        // ACT
        vm.Activate(new());
        vm.SearchQuery = "multi1";
        scheduler.Start();
        
        vm.SelectedAlias.Synonyms = synonyms;
        vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();
        
        vm.Activate(new());
        vm.SearchQuery = "multi1";
        scheduler.AdvanceBy(200.Milliseconds().Ticks);
        
        // ASSERT
        vm.ValidationAliasExists.IsValid.Should().BeTrue();
    });

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
        var loggerFactory = new XUnitLoggerFactory(_output);

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
        await thumbnailManager.RefreshThumbnails(aliases);

        // ASSERT
        connectionMgr.WithinTransaction(tx => (long)tx.Connection.ExecuteScalar("select count(*) from alias_argument"))
                     .Should().Be(6);
    }

    #endregion Methods
}