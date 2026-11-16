using System.Data;
using System.Data.SQLite;
using Dapper;
using Lanceur.Infra.SQLite.ConnectionStrings;
using Lanceur.Tests.Tools;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

/// <summary>
///     Checks that referential integrity is enforced with the production connection settings,
///     and that it does not prevent the database from being created.
/// </summary>
public sealed class ReferentialIntegrityTest : TestBase
{
    #region Constructors

    public ReferentialIntegrityTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    /// <summary>
    ///     Runs <paramref name="action" /> against a database file opened with the production connection string,
    ///     then deletes the file.
    /// </summary>
    private static void WithProductionDatabaseFile(Action<string> action)
    {
        var file = Path.GetTempFileName();
        try { action(file); }
        finally
        {
            // System.Data.SQLite keeps the file locked until pooled connections and statements are finalised.
            SQLiteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var path in new[] { file, $"{file}-wal", $"{file}-shm" })
            {
                try { File.Delete(path); }
                catch (IOException)
                {
                    /* Best effort: a leftover temp file must not fail the test */
                }
            }
        }
    }

    private IDbConnection BuildProductionDb(string file)
        => BuildFreshDb(connectionString: LightConnectionString.FromFile(file));

    [Fact]
    public void When_creating_database_from_scratch_with_foreign_keys_Then_all_scripts_succeed()
        => WithProductionDatabaseFile(file => {
                // ACT
                var action = () => {
                    using var db = BuildProductionDb(file);
                };

                // ASSERT
                action.ShouldNotThrow();
            }
        );

    [Fact]
    public void When_inserting_usage_without_alias_Then_foreign_key_is_enforced()
        => WithProductionDatabaseFile(file => {
                // ARRANGE
                using var db = BuildProductionDb(file);

                // ACT & ASSERT
                Should.Throw<SQLiteException>(() => { db.Execute("insert into alias_usage (id_alias) values (0);"); })
                      .Message.ShouldContain("FOREIGN KEY");
            }
        );

    #endregion
}