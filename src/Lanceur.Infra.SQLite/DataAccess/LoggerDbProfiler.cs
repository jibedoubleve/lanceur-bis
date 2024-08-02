using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using StackExchange.Profiling.Data;

namespace Lanceur.Infra.SQLite.DataAccess;

public abstract class LoggerDbProfiler : IDbProfiler
{
    #region Fields

    protected const string Template = "[{Status}] SQL query: {SqlQuery} - {SqlParameters}";

    #endregion Fields

    #region Constructors

    protected LoggerDbProfiler(bool doFullLogging) => DoFullLogging = doFullLogging;

    #endregion Constructors

    #region Properties

    protected bool DoFullLogging { get; }

    public virtual bool IsActive => true;

    #endregion Properties

    #region Methods

    protected static void Log(IDbCommand profiledDbCommand, Action<string, object[]> log)
    {
        var parameters = profiledDbCommand.Parameters.Cast<SQLiteParameter>()
                                          .Select(x => $"{x.ParameterName}: {x.Value}\r\n")
                                          .ToArray<object>();
        var sql = profiledDbCommand.CommandText;
        log(sql, parameters);
    }

    public virtual void ExecuteFinish(
        IDbCommand profiledDbCommand,
        SqlExecuteType executeType,
        DbDataReader reader
    ) { }

    public virtual void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType) { }
    public virtual void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception) { }

    public virtual void ReaderFinish(IDataReader reader) { }

    #endregion Methods
}