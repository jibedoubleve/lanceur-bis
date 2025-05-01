namespace Lanceur.Core.Models;

public abstract class SelfExecutableQueryResult : ExecutableQueryResult, ISelfExecutable
{
    #region Properties

    public override string Icon => "WindowConsole20";

    #endregion

    #region Methods

    public abstract Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null);

    public override string ToQuery() => $"{Name} {Parameters}".Trim();

    #endregion
}