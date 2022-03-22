using AutoMapper;
using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.Tests.SQLite;
using Lanceur.Utils;
using NSubstitute;
using System.Data.SQLite;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class MacroShould : SQLiteTest
    {
        #region Methods

        [Theory]
        [InlineData("multi", true)]
        [InlineData("MULTI", true)]
        [InlineData("macro", false)]
        [InlineData("MACRO", false)]
        [InlineData("calendar", false)]
        [InlineData("CALENDAR", false)]
        public void BeCompositeAsExpected(string macro, bool expected)
        {
            var alias = new AliasQueryResult { FileName = $"@{macro}@" };

            alias.IsComposite().Should().Be(expected);
        }

        [Fact]
        public void BeMacroComposite()
        {
            // Arrange
            string sql = Cfg.SqlForAliases();
            using var db = BuildFreshDB(sql);
            var service = Cfg.GetDataService(db);

            // Act
            var results = service.Search("alias1");

            // Assert
            (results.ElementAt(0) as CompositeAliasQueryResult).Should().NotBeNull();
        }

        [Fact]
        public void BeRecognisedAsAMacro()
        {
            // Arrange
            var sql = Cfg.SqlForAliases();
            using var db = BuildFreshDB(sql);
            var service = Cfg.GetDataService(db);

            //Act
            var results = service.Search("alias1");

            //Assert
            results.Should().HaveCount(1);
            ((AliasQueryResult)results.ElementAt(0)).IsMacro().Should().BeTrue();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        public void HaveDelayOnFirstElement(int index, int count)
        {
            // Arrange
            string sql = Cfg.SqlForAliases();
            using var db = BuildFreshDB(sql);
            var service = Cfg.GetDataService(db);

            // Act
            var results = service.Search("alias1");

            // Assert
            var composite = (results.ElementAt(0) as CompositeAliasQueryResult);
            composite.Aliases.ElementAt(index).Delay.Should().Be(count);
        }

        [Fact]
        public void HaveExpectedMacroName()
        {
            // Arrange
            var sql = Cfg.SqlForAliases();
            using var db = BuildFreshDB(sql);
            var service = Cfg.GetDataService(db);

            //Act
            var results = service.Search("alias1");

            //Assert
            ((AliasQueryResult)results.ElementAt(0)).GetMacro().Should().Be("MULTI");
        }

        [Fact]
        public void NotBeRecognisedAsAMacro()
        {
            // Arrange
            string sql = Cfg.SqlForAliases();
            using var db = BuildFreshDB(sql);
            var service = Cfg.GetDataService(db);

            // Act
            var results = service.Search("alias2");

            // Assert
            results.Should().HaveCount(1);
            ((AliasQueryResult)results.ElementAt(0)).IsMacro().Should().BeFalse();
        }

        [Theory]
        [InlineData("un")]
        [InlineData("deux")]
        [InlineData("trois")]
        public void RecogniseMacro(string macro)
        {
            var query = new AliasQueryResult() { FileName = $"@{macro}@" };

            query.IsMacro().Should().BeTrue();
        }

        #endregion Methods

        #region Classes

        public static class Cfg
        {
            #region Methods

            public static IConvertionService GetConvertionService()
            {
                var cfg = new MapperConfiguration(c =>
                {
                    c.CreateMap<AliasQueryResult, CompositeAliasQueryResult>();
                });
                return new AutoMapperConverter(new Mapper(cfg));
            }

            public static IDataService GetDataService(SQLiteConnection db)
            {
                var log = Substitute.For<ILogService>();
                var exec = Substitute.For<IExecutionManager>();
                var conv = GetConvertionService();
                var service = new SQLiteDataService(new SQLiteConnectionScope(db), log, exec, conv);
                return service;
            }

            public static string SqlForAliases()
            {
                return @"
                insert into alias (id, file_name, arguments, id_session) values (1, '@multi@', '@alias2@@alias3', 1);
                insert into alias_name (id, id_alias, name) values (1, 1 , 'alias1');

                insert into alias (id, file_name, id_session) values (2, 'c:\dummy\dummy.exe', 1);
                insert into alias_name (id, id_alias, name) values (2, 2, 'alias2');

                insert into alias (id, file_name, id_session) values (3, 'c:\dummy\dummy.exe', 1);
                insert into alias_name (id, id_alias, name) values (3, 3, 'alias3');";
            }

            #endregion Methods
        }

        #endregion Classes
    }
}