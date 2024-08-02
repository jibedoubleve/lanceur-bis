namespace Lanceur.Core.Models;

public abstract class ExecutableQueryResult : QueryResult, IExecutable
{
    #region Properties

    public virtual bool IsElevated { get; set; }

    public string Parameters { get; set; }

    #endregion Properties
}