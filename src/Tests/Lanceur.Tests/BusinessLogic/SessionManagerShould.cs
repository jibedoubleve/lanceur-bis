using System.Data;
using Dapper;
using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Tests.SQLite;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Lanceur.Infra.SQLite.DataAccess;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic
{
    public class SessionManagerShould : SQLiteTest
    {
        #region Constructors

        public SessionManagerShould(ITestOutputHelper output) : base(output)
        {
        }

        #endregion Constructors

        #region Methods

        private static void CreateTableAndPopulate(IDbConnection conn)
        {
            const string sql = @"
            create table alias_session (
                id    integer primary key,
                name  text,
                notes text
            );";

            conn.Execute(sql);

            for (var i = 0; i < 10; i++)
            {
                var insertSql = @"insert into alias_session (name, notes) values ('name_{0}', 'some notes')".Format(i);
                conn.Execute(insertSql);
            }
        }

        [Fact]
        public void HaveTenSessions()
        {
            var scope = new DbSingleConnectionManager(BuildConnection());
            CreateTableAndPopulate(scope.GetConnection());
            var service = new SQLiteRepository(scope, Substitute.For<ILoggerFactory>(), Substitute.For<IConversionService>());

            service.GetSessions().Should().HaveCount(10);
        }

        #endregion Methods
    }
}