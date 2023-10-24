namespace System.IO.FileOps.Infrastructure.Operations;

public abstract class AbstractOperation
{
    #region Constructors

    internal AbstractOperation(string name, Dictionary<string, string> parameters)
    {
        Name = name;
        Parameters = parameters;
    }

    #endregion

    #region Public properties

    public string Name { get; init; }

    public Dictionary<string, string> Parameters { get; init; }

    #endregion
}