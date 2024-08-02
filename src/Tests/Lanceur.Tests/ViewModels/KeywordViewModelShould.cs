using AutoMapper;
using Dapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Humanizer;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Tests.SQLite;
using Lanceur.Tests.Tooling.Builders;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tooling.SQL;
using Lanceur.Utils;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.ViewModels;

public class KeywordViewModelShould : TestBase
{
    #region Constructors

    public KeywordViewModelShould(ITestOutputHelper output) : base(output) { }

    #endregion Constructors

    #region Methods

    [Fact]
    public void BeAbleToRemoveSynonym() => new TestScheduler().With(
        scheduler =>
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

            var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));

            var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
            var cfg = new MapperConfiguration(cfg => { cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>(); });
            var conversionService = new AutoMapperConverter(new Mapper(cfg));
            var dbRepository = new SQLiteRepository(connectionMgr, logger, conversionService);

            var vm = new KeywordsViewModelBuilder().With(scheduler)
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
                             .Should()
                             .Be(2);
                // And the UI also...
                vm.Aliases.Should().HaveCount(2);
            }
        }
    );

    [Fact]
    public void CreateAliasAndSelectIt() => new TestScheduler().With(
        scheduler =>
        {
            // ARRANGE
            var dbRepository = Substitute.For<IDbRepository>();
            dbRepository.SelectNames(Arg.Any<string[]>())
                        .Returns(new ExistingNameResponse(Array.Empty<string>()));

            var vm = new KeywordsViewModelBuilder()
                     .With(scheduler)
                     .With(OutputHelper)
                     .With(dbRepository)
                     .Build();

            // ACT
            vm.Activate(new());
            vm.CreatingAlias.Execute().Subscribe();
            scheduler.Start();

            // Check conditions before going further
            // as it is an error point here.
            vm.SelectedAlias.Should().NotBeNull("because it will be used for the update");

            var hash = vm.SelectedAlias.GetHashCode();

            vm.SelectedAlias.Synonyms = Guid.NewGuid().ToString();
            vm.SelectedAlias.FileName = Guid.NewGuid().ToString();

            vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();

            // ASSERT
            vm.SelectedAlias.GetHashCode().Should().Be(hash);
        }
    );

    [Fact]
    public void CreateAliasForPackagedApp() => new TestScheduler().With(
        scheduler =>
        {
            // ARRANGE
            var packagedAppSearchService = Substitute.For<IPackagedAppSearchService>();
            packagedAppSearchService.GetByInstalledDirectory(Arg.Any<string>())
                                    .Returns(new List<PackagedApp> { new() { AppUserModelId = Guid.NewGuid().ToString() } });

            var vm = new KeywordsViewModelBuilder().With(scheduler)
                                                   .With(OutputHelper)
                                                   .With(packagedAppSearchService)
                                                   .Build();

            // ACT
            vm.Activate(new());
            vm.CreatingAlias.Execute().Subscribe();
            scheduler.Start();

            // Check conditions before going further
            // as it is an error point here.
            vm.SelectedAlias.Should().NotBeNull("because it will be used for the update");

            vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();

            // ASSERT
            vm.SelectedAlias.FileName.Should().StartWith("package:");
        }
    );

    [Theory, InlineData("multi, "), InlineData("multi,")]
    public void NotBeAbleToCreateEmptyAlias(string synonyms) => new TestScheduler().With(
        scheduler =>
        {
            // ARRANGE
            var sql = new SqlBuilder().AppendAlias(10, "@multi@", "@alias2@@alias3")
                                      .AppendSynonyms(10, "multi1", "multi2", "multi3")
                                      .AppendAlias(20, "alias2", "action1")
                                      .AppendSynonyms(20, "alias2")
                                      .AppendAlias(30, "alias3", "action2")
                                      .AppendSynonyms(30, "alias3")
                                      .ToString();

            var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sql));

            var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
            var cfg = new MapperConfiguration(cfg => { cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>(); });
            var conversionService = new AutoMapperConverter(new Mapper(cfg));
            var dbRepository = new SQLiteRepository(connectionMgr, logger, conversionService);

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
        }
    );

    [Theory, InlineData("un,,,,,,deux,,,trois        quatre,          cinq ,,,,,,,     ,,,,,, ,six", 6), InlineData("un deux trois ", 3), InlineData("un  , deux , trois", 3)]
    public void NotBeAbleToSaveEmptySynonyms(string synonyms, int synonymCount) => new TestScheduler().With(
        scheduler =>
        {
            // ARRANGE
            var sqlDDL = new SqlBuilder().AppendAlias(10, Random(), Random())
                                         .AppendSynonyms(10, "alias1")
                                         .AppendAlias(20, Random(), Random())
                                         .AppendSynonyms(20, "alias2")
                                         .AppendAlias(30, Random(), Random())
                                         .AppendSynonyms(30, "alias3")
                                         .ToString();

            var connectionMgr = new DbSingleConnectionManager(BuildFreshDb(sqlDDL));

            var logger = new MicrosoftLoggingLoggerFactory(OutputHelper);
            var cfg = new MapperConfiguration(cfg => { cfg.CreateMap<AliasQueryResult, CompositeAliasQueryResult>(); });
            var conversionService = new AutoMapperConverter(new Mapper(cfg));
            var dbRepository = new SQLiteRepository(connectionMgr, logger, conversionService);

            var vm = new KeywordsViewModelBuilder().With(scheduler)
                                                   .With(logger)
                                                   .With(dbRepository)
                                                   .Build();
            // ACT
            vm.Activate(new());
            vm.SearchQuery = "alias1";
            scheduler.Start();

            vm.SelectedAlias.Synonyms = synonyms;
            vm.SaveOrUpdateAlias.Execute(vm.SelectedAlias).Subscribe();

            vm.Activate(new());
            vm.SearchQuery = "alias1";
            scheduler.AdvanceBy(200.Milliseconds().Ticks);

            // ASSERT
            const string sql = "select count(*) from alias_name where id_alias = 10";
            var count = connectionMgr.WithinTransaction(c => c.Connection.ExecuteScalar(sql));
            count.Should().Be(synonymCount);

            return;

            string Random() => Guid.NewGuid().ToString();
        }
    );

    #endregion Methods
}