namespace Lanceur.Core.Models;

public abstract class SelfExecutableQueryResult : ExecutableQueryResult, ISelfExecutable
{
    #region Constructors

    protected SelfExecutableQueryResult(string name, string description)
    {
        Name = name;
        Description = description;
    }

    protected SelfExecutableQueryResult() { }

    #endregion Constructors

    #region Properties

    public override string Icon => "WindowConsole20";

    #endregion Properties

    #region Methods

    public abstract Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null);

    public override string ToQuery() => $"{Name} {Parameters}".Trim();

    #endregion Methods
}