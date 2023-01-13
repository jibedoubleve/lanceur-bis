using Dapper;
using FluentAssertions;
using Lanceur.Core.Managers;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Tests.SQLite;
using NSubstitute;
using System.Data.SQLite;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class SessionManagerShould : SQLiteTest
    {
        #region Methods

        private static void CreateTableAndPopulate(SQLiteConnection conn)
        {
            var sql = @"
            create table alias_session (
                id    integer primary key,
                name  text,
                notes text
            );";

            conn.Execute(sql);

            for (int i = 0; i < 10; i++)
            {
                string insertSql = @"insert into alias_session (name, notes) values ('name_{0}', 'some notes')".Format(i);
                conn.Execute(insertSql);
            }
        }

        [Fact]
        public void HaveTenSessions()
        {
            var scope = new SQLiteConnectionScope(BuildConnection());
            CreateTableAndPopulate(scope);
            var service = new SQLiteDataService(scope, Substitute.For<ILogService>(), Substitute.For<IConvertionService>());

            service.GetSessions().Should().HaveCount(10);
        }

        #endregion Methods
    }
}