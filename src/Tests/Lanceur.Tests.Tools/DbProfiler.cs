using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using StackExchange.Profiling.Data;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tools;

public class DbProfiler : IDbProfiler
{
    #region Fields

    private readonly ITestOutputHelper _outputHelper;

    private const string Template = """
                                    ----------------
                                    ---- SQL        
                                    ----------------

                                    {0}

                                    ----------------
                                    ---- Parameters 
                                    ----------------

                                    {1}

                                    ------------------------------------------------------------------------------------
                                    """;

    #endregion

    #region Constructors

    public DbProfiler(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    #endregion

    #region Properties

    public bool IsActive { get; set; }

    #endregion

    #region Methods

    private static void Log(IDbCommand profiledDbCommand, Action<string, object[]> log)
    {
        var parameters = profiledDbCommand.Parameters.Cast<SQLiteParameter>()
                                          .Select(x => $"{x.ParameterName}: {x.Value}")
                                          .Cast<object>()
                                          .ToArray();
        var sql = profiledDbCommand.CommandText;
        log(sql, parameters);
    }

    public void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader? reader)
    {
        Log(
            profiledDbCommand,
            (sql, parameters) =>
            {
                _outputHelper.WriteLine($"==== Finished executing command of type {executeType}.");
                _outputHelper.WriteLine(Template, sql, string.Join(Environment.NewLine, parameters));
            }
        );
    }

    public void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType)
    {
        Log(
            profiledDbCommand,
            (sql, parameters) =>
            {
                _outputHelper.WriteLine($"==== Start executing command of type {executeType}.");
                _outputHelper.WriteLine(Template, sql, string.Join(Environment.NewLine, parameters));
            }
        );
    }

    public void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception) => Log(
        profiledDbCommand,
        (sql, parameters) =>
        {
            _outputHelper.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            _outputHelper.WriteLine(@"/!\/!\/!\  ERROR OCCURED /!\/!\/!\");
            _outputHelper.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            _outputHelper.WriteLine(Template, sql, parameters);
        }
    );

    public void ReaderFinish(IDataReader reader) { _outputHelper.WriteLine("Reader finished."); }

    #endregion
}