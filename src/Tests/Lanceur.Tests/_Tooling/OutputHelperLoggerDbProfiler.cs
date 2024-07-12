using System.Data;
using System.Data.Common;
using Lanceur.Infra.SQLite.DataAccess;
using StackExchange.Profiling.Data;
using Xunit.Abstractions;

namespace Lanceur.Tests.Tooling;

public class OutputHelperLoggerDbProfiler : LoggerDbProfiler
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion Fields

    #region Constructors

    public OutputHelperLoggerDbProfiler(ITestOutputHelper output) : base(true)
    {
        _output = output;
    }

    #endregion Constructors

    #region Methods

    public override void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader reader)
    {
        Log(profiledDbCommand, (sql, parameters) => _output.WriteLine($"SQL:\r\n{sql}\r\nParameters:\r\n{parameters}"));
    }

    public override void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception)
    {
        Log(profiledDbCommand, (_, _) => _output.WriteLine("<=============================================>\r\n" +
                                                           "<==============> SQL ON ERROR <===============>\r\n" +
                                                           "<=============================================>"));
    }

    #endregion Methods
}